namespace WPFGame.Level
{
    // Описание одной комнаты целиком: из каких тайлов она состоит
    // и где должен оказаться игрок при входе в неё.
    // Пока без флагов дверей — добавим, когда будем делать переход между комнатами.
    public class RoomTemplate
    {
        public List<TileData> Tiles { get; } = new();

        public double PlayerStartX { get; set; }
        public double PlayerStartY { get; set; }
    }
}
