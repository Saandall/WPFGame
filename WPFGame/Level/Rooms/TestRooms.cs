namespace WPFGame.Level
{
    // Временное место для заготовленных комнат, пока не появится генератор уровня.
    // Room1 — точная копия того, что раньше было нарисовано руками в MainWindow.xaml,
    // чтобы поведение игры не изменилось после рефакторинга.
    public static class TestRooms
    {
        public static RoomTemplate Room1()
        {
            var room = new RoomTemplate
            {
                PlayerStartX = 100,
                PlayerStartY = 100
            };

            // Пол
            room.Tiles.Add(new TileData(TileType.Ground, 0, 350, 900, 50));

            // Балкон (две платформы друг над другом)
            room.Tiles.Add(new TileData(TileType.Platform, 400, 200, 300, 20));
            room.Tiles.Add(new TileData(TileType.Platform, 400, 100, 300, 20));

            // Лестница
            room.Tiles.Add(new TileData(TileType.Ladder, 450, 100, 40, 250));

            // Склоны
            room.Tiles.Add(new TileData(TileType.SlopeUpRight, 222, 200, 178, 150));
            room.Tiles.Add(new TileData(TileType.SlopeUpLeft, 700, 200, 100, 150));

            return room;
        }
    }
}
