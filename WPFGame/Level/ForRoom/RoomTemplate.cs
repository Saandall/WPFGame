namespace WPFGame.Level
{
    // Описание одной комнаты целиком: из каких тайлов она состоит,
    // какого она размера, какие у неё есть двери и куда ставить игрока
    // при входе через ту или иную дверь.
    public class RoomTemplate
    {
        // Уникальное имя комнаты — по нему RoomManager ищет, куда вести дальше
        public string Id { get; set; } = string.Empty;

        // Логический размер комнаты в пикселях (единый для всех — 1920x1080)
        public double Width { get; set; }
        public double Height { get; set; }

        public List<TileData> Tiles { get; } = new();

        // Точка, где появляется игрок, если это САМАЯ ПЕРВАЯ комната уровня
        // (не через дверь, а просто "новая игра началась")
        public double PlayerStartX { get; set; }
        public double PlayerStartY { get; set; }

        // Где у комнаты двери и какой у каждой диапазон (не вся стена целиком).
        // Для Left/Right — диапазон по Y. Для Top/Bottom — диапазон по X.
        public Dictionary<Direction, (double Start, double End)> Doors { get; } = new();

        // Куда поставить игрока, если он вошёл через дверь с конкретной стороны.
        // Например: EntryPoints[Direction.Left] — точка появления, когда игрок
        // зашёл в эту комнату через левую дверь (значит, вышел из предыдущей комнаты вправо).
        public Dictionary<Direction, (double X, double Y)> EntryPoints { get; } = new();
    }
}