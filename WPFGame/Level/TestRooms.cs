namespace WPFGame.Level
{
    // Заготовленные тестовые комнаты, пока нет генератора
    public static class TestRooms
    {
        public static RoomTemplate Room1()
        {
            var room = new RoomTemplate(
                "room1",
                new[] { (Col: 0, Row: 0) })
            {
                PlayerStartX = 100,
                PlayerStartY =
                    RoomMetrics.FloorY -
                    RoomMetrics.DefaultPlayerHeight
            };

            room.Tiles.Add(new TileData(
                TileType.Ground,
                0,
                RoomMetrics.FloorY,
                RoomMetrics.CellWidth,
                RoomMetrics.FloorHeight));

            room.Tiles.Add(new TileData(
                TileType.Platform,
                400,
                RoomMetrics.FloorY - 150,
                300,
                20));

            room.Tiles.Add(new TileData(
                TileType.Platform,
                400,
                RoomMetrics.FloorY - 250,
                300,
                20));


            room.Tiles.Add(new TileData(
                TileType.SlopeUpRight,
                222,
                RoomMetrics.FloorY - 150,
                178,
                150));

            room.Tiles.Add(new TileData(
                TileType.SlopeUpLeft,
                700,
                RoomMetrics.FloorY - 150,
                100,
                150));

            // Одна лестница соединяет пол с верхним проходом по центру блока
            room.Tiles.Add(new TileData(
                TileType.Ladder,
                RoomLayoutRules.GetCenteredLadderX(0),
                0,
                RoomLayoutRules.LadderWidth,
                RoomMetrics.FloorY));

            room.AddDoor(
                Direction.Right,
                0,
                0);

            room.AddDoor(
                Direction.Top,
                0,
                0);

            return room;
        }

        // Горизонтальная комната из двух блоков
        public static RoomTemplate Room2()
        {
            var room = new RoomTemplate(
                "room2",
                new[]
                {
                    (Col: 0, Row: 0),
                    (Col: 1, Row: 0)
                });

            room.Tiles.Add(new TileData(
                TileType.Ground,
                0,
                RoomMetrics.FloorY,
                room.Width,
                RoomMetrics.FloorHeight));

            room.Tiles.Add(new TileData(
                TileType.Platform,
                600,
                RoomMetrics.FloorY - 150,
                150,
                20));

            room.Tiles.Add(new TileData(
                TileType.Platform,
                1200,
                RoomMetrics.FloorY - 150,
                150,
                20));

            room.Tiles.Add(new TileData(
                TileType.Platform,
                1650,
                RoomMetrics.FloorY - 250,
                150,
                20));

            room.AddDoor(
                Direction.Left,
                0,
                0);

            room.AddDoor(
                Direction.Right,
                1,
                0);

            return room;
        }

        // Комната над Room1 с нижней дверью и продолжением лестницы
        public static RoomTemplate Room3()
        {
            var room = new RoomTemplate(
                "room3",
                new[] { (Col: 0, Row: 0) });

            double gapStart =
                RoomMetrics.TopBottomDoorStartX;

            double gapEnd =
                RoomMetrics.TopBottomDoorEndX;

            room.Tiles.Add(new TileData(
                TileType.Ground,
                0,
                RoomMetrics.FloorY,
                gapStart,
                RoomMetrics.FloorHeight));

            room.Tiles.Add(new TileData(
                TileType.Ground,
                gapEnd,
                RoomMetrics.FloorY,
                RoomMetrics.CellWidth - gapEnd,
                RoomMetrics.FloorHeight));

            room.Tiles.Add(new TileData(
                TileType.Platform,
                gapStart,
                RoomMetrics.FloorY,
                RoomMetrics.TopBottomDoorWidth,
                20));

            // Лестница совпадает с центральным проходом нижней комнаты
            room.Tiles.Add(new TileData(
                TileType.Ladder,
                RoomLayoutRules.GetCenteredLadderX(0),
                280,
                RoomLayoutRules.LadderWidth,
                260));

            room.AddDoor(
                Direction.Bottom,
                0,
                0);

            return room;
        }

        // Вертикальная комната из двух блоков
        public static RoomTemplate Room4()
        {
            var room = new RoomTemplate(
                "room4",
                new[]
                {
                    (Col: 0, Row: 0),
                    (Col: 0, Row: 1)
                });

            double floorY =
                room.Height -
                RoomMetrics.FloorHeight;

            room.Tiles.Add(new TileData(
                TileType.Ground,
                0,
                floorY,
                room.Width,
                RoomMetrics.FloorHeight));

            room.Tiles.Add(new TileData(
                TileType.Ladder,
                RoomLayoutRules.GetCenteredLadderX(0),
                0,
                RoomLayoutRules.LadderWidth,
                floorY));

            room.Tiles.Add(new TileData(
                TileType.Platform,
                650,
                350,
                170,
                20));

            room.Tiles.Add(new TileData(
                TileType.Platform,
                200,
                750,
                170,
                20));

            room.AddDoor(
                Direction.Left,
                0,
                1);

            room.AddDoor(
                Direction.Right,
                0,
                1);

            return room;
        }

        // Прямоугольная комната размером два на два блока
        public static RoomTemplate Room5()
        {
            var room = RoomBuilder.Build(
                "room5",
                new[]
                {
                    (Col: 0, Row: 0),
                    (Col: 1, Row: 0),
                    (Col: 0, Row: 1),
                    (Col: 1, Row: 1)
                },
                new[]
                {
                    (
                        Direction: Direction.Left,
                        CellCol: 0,
                        CellRow: 1
                    ),
                    (
                        Direction: Direction.Right,
                        CellCol: 1,
                        CellRow: 1
                    )
                });

            double floorY =
                room.Height -
                RoomMetrics.FloorHeight;

            // Лестница позволяет проверить вертикальное движение камеры
            room.Tiles.Add(new TileData(
                TileType.Ladder,
                RoomLayoutRules.GetCenteredLadderX(0),
                0,
                RoomLayoutRules.LadderWidth,
                floorY));

            // Платформы обозначают содержимое верхних и нижних блоков
            room.Tiles.Add(new TileData(
                TileType.Platform,
                180,
                350,
                260,
                20));

            room.Tiles.Add(new TileData(
                TileType.Platform,
                1220,
                350,
                260,
                20));

            room.Tiles.Add(new TileData(
                TileType.Platform,
                1250,
                800,
                260,
                20));

            return room;
        }

        // Г-образная комната из трёх блоков
        public static RoomTemplate Room6()
        {
            var room = RoomBuilder.Build(
                "room6",
                new[]
                {
                    (Col: 0, Row: 0),
                    (Col: 1, Row: 0),
                    (Col: 0, Row: 1)
                },
                new[]
                {
                    (
                        Direction: Direction.Left,
                        CellCol: 0,
                        CellRow: 1
                    )
                });

            double lowerFloorY =
                room.Height -
                RoomMetrics.FloorHeight;

            // Лестница соединяет нижний левый блок с верхним рядом
            room.Tiles.Add(new TileData(
                TileType.Ladder,
                RoomLayoutRules.GetCenteredLadderX(0),
                0,
                RoomLayoutRules.LadderWidth,
                lowerFloorY));

            // Платформа обозначает верхний правый блок
            room.Tiles.Add(new TileData(
                TileType.Platform,
                RoomMetrics.CellWidth + 220,
                300,
                260,
                20));

            return room;
        }
    }
}
