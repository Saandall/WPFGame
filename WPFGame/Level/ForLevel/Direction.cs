namespace WPFGame.Level
{
    // Стороны комнаты и направления дверных проходов
    public enum Direction
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public static class DirectionExtensions
    {
        // Возвращает противоположное направление для совмещения дверей
        public static Direction Opposite(
            this Direction direction)
        {
            return direction switch
            {
                Direction.Left =>
                    Direction.Right,

                Direction.Right =>
                    Direction.Left,

                Direction.Top =>
                    Direction.Bottom,

                Direction.Bottom =>
                    Direction.Top,

                _ =>
                    direction
            };
        }
    }
}
