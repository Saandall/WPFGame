namespace WPFGame.Level
{
    // Собирает стандартную оболочку комнаты из блоков
    public static class RoomBuilder
    {
        // получаем id комнаты, задействованные блоки и двери комнаты
        public static RoomTemplate Build(
            string id,
            IEnumerable<(int Col, int Row)> occupiedCells,
            IEnumerable<(
                Direction Direction,
                int CellCol,
                int CellRow)> doors)
        {
            // создаём новую комнату по входным данным
            var room = new RoomTemplate(
                id,
                occupiedCells);
            
            ValidateConnectedShape(room);

            // Двери добавляются до добавления пола, чтобы учесть нижние проходы
            foreach (var door in doors)
            {
                room.AddDoor(
                    door.Direction,
                    door.CellCol,
                    door.CellRow);
            }

            AddBottomFloors(room);
            AddOuterBoundaries(room);

            return room;
        }

        // Проверяет, что все блоки соединены общей стороной
        private static void ValidateConnectedShape(
            RoomTemplate room)
        {
            var firstCell =
                room.OccupiedCells.First();
            
            // множество просмотренных блоков
            var visited =
                new HashSet<(int Col, int Row)>();
            
            // очередь из блоков
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
                RoomLayoutRules.PlatformHeight));
        }


        // Добавляет стены и потолок только по внешним сторонам блоков комнаты
        private static void AddOuterBoundaries(
            RoomTemplate room)
        {
            foreach (var cell in
                     room.OccupiedCells)
            {
                var leftCell =
                    (Col: cell.Col - 1, Row: cell.Row);

                if (!room.OccupiedCells.Contains(
                        leftCell))
                {
                    AddSideBoundary(
                        room,
                        cell.Col,
                        cell.Row,
                        Direction.Left);
                }

                var rightCell =
                    (Col: cell.Col + 1, Row: cell.Row);

                if (!room.OccupiedCells.Contains(
                        rightCell))
                {
                    AddSideBoundary(
                        room,
                        cell.Col,
                        cell.Row,
                        Direction.Right);
                }

                var topCell =
                    (Col: cell.Col, Row: cell.Row - 1);

                if (!room.OccupiedCells.Contains(
                        topCell))
                {
                    AddTopBoundary(
                        room,
                        cell.Col,
                        cell.Row);
                }
            }
        }

        // Создаёт боковую стену и оставляет проём у активной боковой двери
        private static void AddSideBoundary(
            RoomTemplate room,
            int cellCol,
            int cellRow,
            Direction direction)
        {
            DoorSlot? door =
                room.Doors.FirstOrDefault(
                    door =>
                        door.Direction ==
                            direction &&
                        door.CellCol ==
                            cellCol &&
                        door.CellRow ==
                            cellRow);

            double cellX =
                cellCol *
                RoomMetrics.CellWidth;

            double cellY =
                cellRow *
                RoomMetrics.CellHeight;

            double wallX =
                direction ==
                Direction.Left
                    ? cellX
                    : cellX +
                      RoomMetrics.CellWidth -
                      RoomMetrics.BoundaryThickness;

            if (door is null)
            {
                room.Tiles.Add(
                    new TileData(
                        TileType.Ground,
                        wallX,
                        cellY,
                        RoomMetrics.BoundaryThickness,
                        RoomMetrics.CellHeight));

                return;
            }

            double doorStartY =
                cellY +
                RoomMetrics.FloorY -
                RoomMetrics.SideDoorHeight;

            double doorEndY =
                cellY +
                RoomMetrics.FloorY +
                RoomLayoutRules.PlatformHeight;

            if (doorStartY >
                cellY)
            {
                room.Tiles.Add(
                    new TileData(
                        TileType.Ground,
                        wallX,
                        cellY,
                        RoomMetrics.BoundaryThickness,
                        doorStartY -
                            cellY));
            }

            double cellBottom =
                cellY +
                RoomMetrics.CellHeight;

            if (doorEndY <
                cellBottom)
            {
                room.Tiles.Add(
                    new TileData(
                        TileType.Ground,
                        wallX,
                        doorEndY,
                        RoomMetrics.BoundaryThickness,
                        cellBottom -
                            doorEndY));
            }
        }

        // Создаёт потолок и оставляет проход у активной верхней двери
        private static void AddTopBoundary(
            RoomTemplate room,
            int cellCol,
            int cellRow)
        {
            DoorSlot? topDoor =
                room.Doors.FirstOrDefault(
                    door =>
                        door.Direction ==
                            Direction.Top &&
                        door.CellCol ==
                            cellCol &&
                        door.CellRow ==
                            cellRow);

            double cellX =
                cellCol *
                RoomMetrics.CellWidth;

            double cellY =
                cellRow *
                RoomMetrics.CellHeight;

            if (topDoor is null)
            {
                room.Tiles.Add(
                    new TileData(
                        TileType.Ground,
                        cellX,
                        cellY,
                        RoomMetrics.CellWidth,
                        RoomMetrics.BoundaryThickness));

                return;
            }

            room.Tiles.Add(
                new TileData(
                    TileType.Ground,
                    cellX,
                    cellY,
                    RoomMetrics.TopBottomDoorStartX,
                    RoomMetrics.BoundaryThickness));

            room.Tiles.Add(
                new TileData(
                    TileType.Ground,
                    cellX +
                        RoomMetrics.TopBottomDoorEndX,
                    cellY,
                    RoomMetrics.CellWidth -
                        RoomMetrics.TopBottomDoorEndX,
                    RoomMetrics.BoundaryThickness));
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
