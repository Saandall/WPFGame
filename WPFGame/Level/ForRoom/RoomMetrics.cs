namespace WPFGame.Level
{
    // Общие размеры блоков, пола и дверных проёмов
    public static class RoomMetrics
    {
        public const double CellWidth = 960;
        public const double CellHeight = 540;

        public const int MaxCellsWide = 2;
        public const int MaxCellsHigh = 2;

        public const double FloorHeight = 80;
        public const double FloorY = CellHeight - FloorHeight;
        public const double BoundaryThickness = 30;

        public const double SideDoorHeight = 70;

        public const double TopBottomDoorWidth = 180;
        public const double TopBottomDoorStartX =
            (CellWidth - TopBottomDoorWidth) / 2;

        public const double TopBottomDoorEndX =
            TopBottomDoorStartX + TopBottomDoorWidth;

        public const double DoorTriggerDepth = 30;

        public const double DefaultPlayerHeight = 50;
    }
}
