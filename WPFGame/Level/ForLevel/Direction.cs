namespace WPFGame.Level
{
    // перечисление сторон, используется там где нужно указать направление двери
    public enum Direction
    {
        Left,
        Right,
        Top,
        Bottom
    }

    //
    public static class DirectionExtensions
    {
        // Если вышел из комнаты вправо — в следующую заходишь слева
        // используется для сопоставления дверей
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