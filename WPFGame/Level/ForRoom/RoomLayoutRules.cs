namespace WPFGame.Level
{
    // Стандартные правила размещения содержимого внутри блока комнаты
    public static class RoomLayoutRules
    {
        public const double LadderWidth = 40;

        // Лестница к верхнему или нижнему проходу ставится по центру блока
        public static double GetCenteredLadderX(
            int cellCol)
        {
            if (cellCol < 0 ||
                cellCol >= RoomMetrics.MaxCellsWide)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cellCol));
            }

            return cellCol *
                   RoomMetrics.CellWidth +
                   (RoomMetrics.CellWidth -
                    LadderWidth) / 2;
        }
    }
}
