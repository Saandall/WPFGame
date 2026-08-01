namespace WPFGame.Level
{
    // Одна "запись" про один тайл: что это, где находится, какого размера.
    public readonly struct TileData
    {
        public TileType Type { get; }
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public TileData(TileType type, double x, double y, double width, double height)
        {
            Type = type;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
