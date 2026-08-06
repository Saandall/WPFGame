namespace WPFGame.Level
{
    // Возможное место двери на внешней стороне блока
    public sealed class DoorSlot
    {
        public string Id =>
            $"{Direction.ToString().ToLowerInvariant()}_{CellCol}_{CellRow}";

        public Direction Direction { get; }
        public int CellCol { get; }
        public int CellRow { get; }

        public DoorSlot(
            Direction direction,
            int cellCol,
            int cellRow)
        {
            if (cellCol < 0 || cellCol >= RoomMetrics.MaxCellsWide)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cellCol),
                    $"Номер столбца должен быть от 0 до {RoomMetrics.MaxCellsWide - 1}.");
            }

            if (cellRow < 0 || cellRow >= RoomMetrics.MaxCellsHigh)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cellRow),
                    $"Номер строки должен быть от 0 до {RoomMetrics.MaxCellsHigh - 1}.");
            }

            Direction = direction;
            CellCol = cellCol;
            CellRow = cellRow;
        }
    }
}
