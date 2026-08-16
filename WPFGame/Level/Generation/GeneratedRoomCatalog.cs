namespace WPFGame.Level
{
    // Создаёт набор форм, доступных первому генератору
    public static class GeneratedRoomCatalog
    {
        public static IReadOnlyList<
            GeneratedRoomDefinition> CreateDefault()
        {
            return new[]
            {
                CreateCompact(),
                CreateWide(),
                CreateTall(),
                CreateLarge()
            };
        }

        // Комната один на один с дверью на любой стороне
        private static GeneratedRoomDefinition
            CreateCompact()
        {
            return new GeneratedRoomDefinition(
                "compact_1x1",
                new[]
                {
                    (Col: 0, Row: 0)
                },
                new[]
                {
                    new DoorSlot(
                        Direction.Left,
                        0,
                        0),

                    new DoorSlot(
                        Direction.Right,
                        0,
                        0),

                    new DoorSlot(
                        Direction.Top,
                        0,
                        0),

                    new DoorSlot(
                        Direction.Bottom,
                        0,
                        0)
                },
                GeneratedRoomStyle.Compact);
        }

        // Комната два на один с вертикальными дверями в любом блоке
        private static GeneratedRoomDefinition
            CreateWide()
        {
            return new GeneratedRoomDefinition(
                "wide_2x1",
                new[]
                {
                    (Col: 0, Row: 0),
                    (Col: 1, Row: 0)
                },
                new[]
                {
                    new DoorSlot(
                        Direction.Left,
                        0,
                        0),

                    new DoorSlot(
                        Direction.Right,
                        1,
                        0),

                    new DoorSlot(
                        Direction.Top,
                        0,
                        0),

                    new DoorSlot(
                        Direction.Top,
                        1,
                        0),

                    new DoorSlot(
                        Direction.Bottom,
                        0,
                        0),

                    new DoorSlot(
                        Direction.Bottom,
                        1,
                        0)
                },
                GeneratedRoomStyle.Wide);
        }

        // Комната один на два с боковыми дверями на обоих этажах
        private static GeneratedRoomDefinition
            CreateTall()
        {
            return new GeneratedRoomDefinition(
                "tall_1x2",
                new[]
                {
                    (Col: 0, Row: 0),
                    (Col: 0, Row: 1)
                },
                new[]
                {
                    new DoorSlot(
                        Direction.Left,
                        0,
                        0),

                    new DoorSlot(
                        Direction.Left,
                        0,
                        1),

                    new DoorSlot(
                        Direction.Right,
                        0,
                        0),

                    new DoorSlot(
                        Direction.Right,
                        0,
                        1),

                    new DoorSlot(
                        Direction.Top,
                        0,
                        0),

                    new DoorSlot(
                        Direction.Bottom,
                        0,
                        1)
                },
                GeneratedRoomStyle.Tall);
        }

        // Комната два на два с дверями по всему внешнему контуру
        private static GeneratedRoomDefinition
            CreateLarge()
        {
            return new GeneratedRoomDefinition(
                "large_2x2",
                new[]
                {
                    (Col: 0, Row: 0),
                    (Col: 1, Row: 0),
                    (Col: 0, Row: 1),
                    (Col: 1, Row: 1)
                },
                new[]
                {
                    new DoorSlot(
                        Direction.Left,
                        0,
                        0),

                    new DoorSlot(
                        Direction.Left,
                        0,
                        1),

                    new DoorSlot(
                        Direction.Right,
                        1,
                        0),

                    new DoorSlot(
                        Direction.Right,
                        1,
                        1),

                    new DoorSlot(
                        Direction.Top,
                        0,
                        0),

                    new DoorSlot(
                        Direction.Top,
                        1,
                        0),

                    new DoorSlot(
                        Direction.Bottom,
                        0,
                        1),

                    new DoorSlot(
                        Direction.Bottom,
                        1,
                        1)
                },
                GeneratedRoomStyle.Large);
        }
    }
}
