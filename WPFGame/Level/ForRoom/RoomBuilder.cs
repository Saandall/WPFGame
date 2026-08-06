namespace WPFGame.Level
{
    // Собирает стандартную оболочку комнаты из блоков
    public static class RoomBuilder
    {
        public static RoomTemplate Build(
            string id,
            IEnumerable<(int Col, int Row)> occupiedCells,
            IEnumerable<(
                Direction Direction,
                int CellCol,
                int CellRow)> doors)
        {
            var room = new RoomTemplate(
                id,
                occupiedCells);

            ValidateConnectedShape(room);

            // Двери добавляются до пола, чтобы учесть нижние проходы
            foreach (var door in doors)
            {
                room.AddDoor(
                    door.Direction,
                    door.CellCol,
                    door.CellRow);
            }

            AddBottomFloors(room);

            return room;
        }

        // Проверяет, что все блоки соединены общей стороной
        private static void ValidateConnectedShape(
            RoomTemplate room)
        {
            var firstCell =
                room.OccupiedCells.First();

            var visited =
                new HashSet<(int Col, int Row)>();

            var queue =
                new Queue<(int Col, int Row)>();

            visited.Add(firstCell);
            queue.Enqueue(firstCell);

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();

                foreach (var neighbour in
                         GetNeighbours(cell))
                {
                    if (!room.OccupiedCells.Contains(neighbour) ||
                        !visited.Add(neighbour))
                    {
                        continue;
                    }

                    queue.Enqueue(neighbour);
                }
            }

            if (visited.Count !=
                room.OccupiedCells.Count)
            {
                throw new InvalidOperationException(
                    $"Блоки комнаты {room.Id} не образуют единую форму.");
            }
        }

        // Добавляет пол только под нижними внешними сторонами блоков
        private static void AddBottomFloors(
            RoomTemplate room)
        {
            foreach (var cell in room.OccupiedCells)
            {
                var cellBelow =
                    (Col: cell.Col, Row: cell.Row + 1);

                if (room.OccupiedCells.Contains(cellBelow))
                {
                    continue;
                }

                var bottomDoor =
                    room.Doors.FirstOrDefault(
                        door =>
                            door.Direction ==
                                Direction.Bottom &&
                            door.CellCol == cell.Col &&
                            door.CellRow == cell.Row);

                if (bottomDoor is null)
                {
                    AddSolidFloor(
                        room,
                        cell.Col,
                        cell.Row);
                }
                else
                {
                    AddFloorWithBottomPassage(
                        room,
                        cell.Col,
                        cell.Row);
                }
            }
        }

        // Создаёт сплошной пол шириной в один блок
        private static void AddSolidFloor(
            RoomTemplate room,
            int cellCol,
            int cellRow)
        {
            room.Tiles.Add(new TileData(
                TileType.Ground,
                cellCol * RoomMetrics.CellWidth,
                cellRow * RoomMetrics.CellHeight +
                    RoomMetrics.FloorY,
                RoomMetrics.CellWidth,
                RoomMetrics.FloorHeight));
        }

        // Оставляет проход в полу под нижней дверью
        private static void AddFloorWithBottomPassage(
            RoomTemplate room,
            int cellCol,
            int cellRow)
        {
            double cellX =
                cellCol *
                RoomMetrics.CellWidth;

            double floorY =
                cellRow *
                RoomMetrics.CellHeight +
                RoomMetrics.FloorY;

            room.Tiles.Add(new TileData(
                TileType.Ground,
                cellX,
                floorY,
                RoomMetrics.TopBottomDoorStartX,
                RoomMetrics.FloorHeight));

            room.Tiles.Add(new TileData(
                TileType.Ground,
                cellX +
                    RoomMetrics.TopBottomDoorEndX,
                floorY,
                RoomMetrics.CellWidth -
                    RoomMetrics.TopBottomDoorEndX,
                RoomMetrics.FloorHeight));

            // Односторонняя платформа закрывает проход при обычной ходьбе
            room.Tiles.Add(new TileData(
                TileType.Platform,
                cellX +
                    RoomMetrics.TopBottomDoorStartX,
                floorY,
                RoomMetrics.TopBottomDoorWidth,
                20));
        }

        // Возвращает соседние позиции блока по четырём сторонам
        private static IEnumerable<(
            int Col,
            int Row)> GetNeighbours(
                (int Col, int Row) cell)
        {
            yield return (
                cell.Col - 1,
                cell.Row);

            yield return (
                cell.Col + 1,
                cell.Row);

            yield return (
                cell.Col,
                cell.Row - 1);

            yield return (
                cell.Col,
                cell.Row + 1);
        }
    }
}
