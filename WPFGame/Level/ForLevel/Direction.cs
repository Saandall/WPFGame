namespace WPFGame.Level
{
    public enum Direction
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public static class DirectionExtensions
    {
        // Если вышел из комнаты вправо — в следующую заходишь как бы "слева".
        // Нужно, чтобы понять, в какую EntryPoint ставить игрока в новой комнате.
        public static Direction Opposite(this Direction direction) => direction switch
        {
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            Direction.Top => Direction.Bottom,
            Direction.Bottom => Direction.Top,
            _ => direction
        };
    }
}