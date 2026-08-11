namespace WPFGame.Core
{
    // Определяет порядок отрисовки объектов на игровом Canvas
    public static class ZLayer
    {
        public const int Tiles = 0;
        public const int Enemies = 8;
        public const int Player = 10;
        public const int Projectiles = 15;
        public const int Slopes = 20;
        public const int Interface = 100;
    }
}
