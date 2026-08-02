namespace WPFGame.Level
{
    // Заготовленные тестовые комнаты, пока нет генератора.
    public static class TestRooms
    {
        private const double PlayerHeight = 50;
        private const double FloorY = 460;      // пол прижат к низу комнаты (высота комнаты 540)
        private const double FloorHeight = 80;
        private const double DoorSize = 70;     // высота/ширина проёма — чуть больше роста игрока

        public static RoomTemplate Room1()
        {
            var room = new RoomTemplate { Id = "room1", Width = 960, Height = 540, PlayerStartX = 100, PlayerStartY = FloorY - PlayerHeight };

            room.Tiles.Add(new TileData(TileType.Ground, 0, FloorY, 960, FloorHeight));

            room.Tiles.Add(new TileData(TileType.Platform, 400, FloorY - 150, 300, 20));
            room.Tiles.Add(new TileData(TileType.Platform, 400, FloorY - 250, 300, 20));
            room.Tiles.Add(new TileData(TileType.Ladder, 450, FloorY - 250, 40, 250));
            room.Tiles.Add(new TileData(TileType.SlopeUpRight, 222, FloorY - 150, 178, 150));
            room.Tiles.Add(new TileData(TileType.SlopeUpLeft, 700, FloorY - 150, 100, 150));

            // Лестница до потолка — путь наверх, в Room3 (сдвинута с 1000 на 850, чтобы влезть в 960 шириной)
            room.Tiles.Add(new TileData(TileType.Ladder, 850, 0, 40, FloorY));

            room.Doors[Direction.Right] = (FloorY - DoorSize, FloorY);
            room.EntryPoints[Direction.Right] = (960 - DoorSize, FloorY - PlayerHeight);

            room.Doors[Direction.Top] = (850 - 20, 850 + 40 + 20);
            room.EntryPoints[Direction.Top] = (850, 20);

            return room;
        }

        // Большая комната — специально шире экрана (2400 против viewport 960), чтобы
        // проверить, как камера следует за игроком и упирается в границы комнаты.
        public static RoomTemplate Room2()
        {
            var room = new RoomTemplate { Id = "room2", Width = 2400, Height = 540 };

            room.Tiles.Add(new TileData(TileType.Ground, 0, FloorY, 2400, FloorHeight));

            // Платформы просто как визуальные ориентиры, чтобы движение камеры было заметно
            room.Tiles.Add(new TileData(TileType.Platform, 600, FloorY - 150, 150, 20));
            room.Tiles.Add(new TileData(TileType.Platform, 1200, FloorY - 150, 150, 20));
            room.Tiles.Add(new TileData(TileType.Platform, 1800, FloorY - 150, 150, 20));

            room.Doors[Direction.Left] = (FloorY - DoorSize, FloorY);
            room.EntryPoints[Direction.Left] = (DoorSize, FloorY - PlayerHeight);

            room.Doors[Direction.Right] = (FloorY - DoorSize, FloorY);
            room.EntryPoints[Direction.Right] = (2400 - DoorSize, FloorY - PlayerHeight);

            return room;
        }

        // Комната, большая и по ширине, и по высоте — проверить упор камеры во все 4 края.
        public static RoomTemplate Room4()
        {
            var room = new RoomTemplate { Id = "room4", Width = 1200, Height = 1600 };

            const double tallFloorY = 1520; // своя высота пола — комната намного выше остальных

            room.Tiles.Add(new TileData(TileType.Ground, 0, tallFloorY, 1200, 80));

            // Лестница на всю высоту комнаты — для проверки вертикальной камеры
            room.Tiles.Add(new TileData(TileType.Ladder, 580, 0, 40, tallFloorY));

            // Платформы как визуальные ориентиры по пути наверх
            room.Tiles.Add(new TileData(TileType.Platform, 750, 400, 150, 20));
            room.Tiles.Add(new TileData(TileType.Platform, 300, 900, 150, 20));

            room.Doors[Direction.Left] = (tallFloorY - DoorSize, tallFloorY);
            room.EntryPoints[Direction.Left] = (DoorSize, tallFloorY - PlayerHeight);

            return room;
        }

        // Комната сверху от Room1. Пол разорван провальной платформой посередине.
        public static RoomTemplate Room3()
        {
            var room = new RoomTemplate { Id = "room3", Width = 960, Height = 540 };

            const double gapStart = 390;
            const double gapWidth = 180;
            const double gapEnd = gapStart + gapWidth;

            room.Tiles.Add(new TileData(TileType.Ground, 0, FloorY, gapStart, FloorHeight));
            room.Tiles.Add(new TileData(TileType.Ground, gapEnd, FloorY, 960 - gapEnd, FloorHeight));
            room.Tiles.Add(new TileData(TileType.Platform, gapStart, FloorY, gapWidth, 20));

            room.Doors[Direction.Bottom] = (gapStart, gapEnd);
            room.EntryPoints[Direction.Bottom] = (60, FloorY - PlayerHeight);

            return room;
        }
    }
}