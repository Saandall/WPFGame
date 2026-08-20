namespace WPFGame.Level
{
    // Хранит форму, геометрию и точки взаимодействия одной заготовки комнаты
    public class RoomTemplate
    {
        private readonly List<DoorSlot> doors =
            new();

        private readonly HashSet<(
            int Col,
            int Row)> occupiedCellSet =
            new();

        public string Id { get; }
        public double Width { get; }
        public double Height { get; }

        public List<TileData> Tiles { get; } =
            new();

        public IReadOnlyList<DoorSlot> Doors =>
            doors;

        // Множество блоков, из которых состоит форма комнаты
        public IReadOnlySet<(
            int Col,
            int Row)> OccupiedCells =>
            occupiedCellSet;

        // Начальная позиция в первой комнате уровня
        public double PlayerStartX { get; set; }
        public double PlayerStartY { get; set; }

        // Создаёт шаблон комнаты из набора занятых блоков
        public RoomTemplate(
            string id,
            IEnumerable<(int Col, int Row)> occupiedCells)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "ID комнаты не должен быть пустым.",
                    nameof(id));
            }

            Id = id;

            int maxCol =
                -1;

            int maxRow =
                -1;

            foreach (var cell in
                     occupiedCells)
            {
                ValidateCell(
                    cell.Col,
                    cell.Row);

                if (!occupiedCellSet.Add(
                        cell))
                {
                    throw new ArgumentException(
                        $"Блок ({cell.Col}, {cell.Row}) указан дважды.",
                        nameof(occupiedCells));
                }

                maxCol =
                    Math.Max(
                        maxCol,
                        cell.Col);

                maxRow =
                    Math.Max(
                        maxRow,
                        cell.Row);
            }

            if (occupiedCellSet.Count == 0)
            {
                throw new ArgumentException(
                    "Комната должна содержать хотя бы один блок.",
                    nameof(occupiedCells));
            }

            Width =
                (maxCol + 1) *
                RoomMetrics.CellWidth;

            Height =
                (maxRow + 1) *
                RoomMetrics.CellHeight;
        }

        // Добавляет дверь только на внешнюю сторону существующего блока
        public DoorSlot AddDoor(
            Direction direction,
            int cellCol,
            int cellRow)
        {
            if (!occupiedCellSet.Contains(
                    (cellCol, cellRow)))
            {
                throw new InvalidOperationException(
                    $"Нельзя добавить дверь: блока ({cellCol}, {cellRow}) нет в комнате {Id}.");
            }

            var adjacentCell =
                GetAdjacentCell(
                    direction,
                    cellCol,
                    cellRow);

            if (occupiedCellSet.Contains(
                    adjacentCell))
            {
                throw new InvalidOperationException(
                    $"Нельзя добавить дверь {direction} на блок ({cellCol}, {cellRow}): " +
                    "это внутренняя граница между двумя блоками комнаты.");
            }

            foreach (var existingDoor in
                     doors)
            {
                if (existingDoor.Direction ==
                        direction &&
                    existingDoor.CellCol ==
                        cellCol &&
                    existingDoor.CellRow ==
                        cellRow)
                {
                    throw new InvalidOperationException(
                        $"Дверь {existingDoor.Id} уже добавлена в комнату {Id}.");
                }
            }

            var door =
                new DoorSlot(
                    direction,
                    cellCol,
                    cellRow);

            doors.Add(
                door);

            return door;
        }

        // Возвращает дверь по её id
        public DoorSlot? GetDoor(
            string doorId)
        {
            foreach (var door in
                     doors)
            {
                if (door.Id ==
                    doorId)
                {
                    return door;
                }
            }

            return null;
        }

        // Проверяет корректность положения блока в максимальной сетке комнаты
        private static void ValidateCell(
            int cellCol,
            int cellRow)
        {
            if (cellCol < 0 ||
                cellCol >=
                RoomMetrics.MaxCellsWide)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cellCol),
                    $"Столбец блока должен быть от 0 до {RoomMetrics.MaxCellsWide - 1}.");
            }

            if (cellRow < 0 ||
                cellRow >=
                RoomMetrics.MaxCellsHigh)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cellRow),
                    $"Строка блока должна быть от 0 до {RoomMetrics.MaxCellsHigh - 1}.");
            }
        }

        // Возвращает соседнюю клетку с указанной стороны блока
        private static (
            int Col,
            int Row) GetAdjacentCell(
                Direction direction,
                int cellCol,
                int cellRow)
        {
            return direction switch
            {
                Direction.Left =>
                    (cellCol - 1, cellRow),

                Direction.Right =>
                    (cellCol + 1, cellRow),

                Direction.Top =>
                    (cellCol, cellRow - 1),

                Direction.Bottom =>
                    (cellCol, cellRow + 1),

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(direction))
            };
        }
    }
}
