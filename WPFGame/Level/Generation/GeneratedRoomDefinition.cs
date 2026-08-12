namespace WPFGame.Level
{
    // Определяет форму и допустимые позиции дверей генерируемой комнаты
    public class GeneratedRoomDefinition
    {
        private readonly GeneratedRoomStyle style;

        public string Id { get; }

        public IReadOnlyCollection<(
            int Col,
            int Row)> OccupiedCells
        {
            get;
        }

        public IReadOnlyList<DoorSlot> PotentialDoors
        {
            get;
        }

        public GeneratedRoomDefinition(
            string id,
            IEnumerable<(
                int Col,
                int Row)> occupiedCells,
            IEnumerable<DoorSlot> potentialDoors,
            GeneratedRoomStyle style)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "ID определения комнаты не должен быть пустым.",
                    nameof(id));
            }

            Id = id;

            OccupiedCells =
                occupiedCells
                    .Distinct()
                    .ToArray();

            PotentialDoors =
                potentialDoors
                    .ToArray();

            this.style =
                style;

            if (OccupiedCells.Count == 0)
            {
                throw new ArgumentException(
                    "Определение комнаты должно содержать блоки.",
                    nameof(occupiedCells));
            }

            if (PotentialDoors.Count == 0)
            {
                throw new ArgumentException(
                    "Определение комнаты должно содержать позиции дверей.",
                    nameof(potentialDoors));
            }
        }

        // Возвращает допустимые двери указанного направления
        public IEnumerable<DoorSlot> GetPotentialDoors(
            Direction direction)
        {
            return PotentialDoors.Where(
                door =>
                    door.Direction ==
                    direction);
        }

        // Создаёт шаблон только с дверями, выбранными генератором
        public RoomTemplate CreateTemplate(
            string templateId,
            IEnumerable<DoorSlot> activeDoors)
        {
            var selectedDoors =
                activeDoors.ToList();

            if (selectedDoors.Count == 0)
            {
                throw new ArgumentException(
                    "Генерируемая комната должна иметь хотя бы одну активную дверь.",
                    nameof(activeDoors));
            }

            var selectedDoorIds =
                new HashSet<string>();

            foreach (var door in
                     selectedDoors)
            {
                bool isPotentialDoor =
                    PotentialDoors.Any(
                        potentialDoor =>
                            potentialDoor.Id ==
                                door.Id &&
                            potentialDoor.Direction ==
                                door.Direction &&
                            potentialDoor.CellCol ==
                                door.CellCol &&
                            potentialDoor.CellRow ==
                                door.CellRow);

                if (!isPotentialDoor)
                {
                    throw new InvalidOperationException(
                        $"Дверь {door.Id} не поддерживается определением {Id}.");
                }

                if (!selectedDoorIds.Add(
                        door.Id))
                {
                    throw new InvalidOperationException(
                        $"Дверь {door.Id} выбрана дважды.");
                }
            }

            var room =
                RoomBuilder.Build(
                    templateId,
                    OccupiedCells,
                    selectedDoors.Select(
                        door => (
                            door.Direction,
                            door.CellCol,
                            door.CellRow)));

            // Для проверки полной коллизии Ground строятся внешние стены и потолок.
            // Нижняя сторона по-прежнему создаётся RoomBuilder.
            AddSolidOuterShell(
                room,
                selectedDoors);

            room.PlayerStartX =
                100;

            room.PlayerStartY =
                room.Height -
                RoomMetrics.FloorHeight -
                RoomMetrics.DefaultPlayerHeight;

            AddVerticalAccess(
                room,
                selectedDoors);

            AddPotentialSideDoorPlatforms(
                room);

            AddDecorations(
                room);

            // Дополняет сквозные платформы небольшим полностью твёрдым элементом.
            AddSolidCollisionSample(
                room);

            return room;
        }

        // Добавляет Ground только по внешним левым, правым и верхним сторонам блоков
        private void AddSolidOuterShell(
            RoomTemplate room,
            IReadOnlyCollection<DoorSlot> activeDoors)
        {
            const double boundaryThickness =
                30;

            var occupiedCells =
                OccupiedCells.ToHashSet();

            foreach (var cell in
                     OccupiedCells)
            {
                var leftCell =
                    (Col: cell.Col - 1, Row: cell.Row);

                if (!occupiedCells.Contains(
                        leftCell))
                {
                    AddSideBoundary(
                        room,
                        cell.Col,
                        cell.Row,
                        Direction.Left,
                        activeDoors,
                        boundaryThickness);
                }

                var rightCell =
                    (Col: cell.Col + 1, Row: cell.Row);

                if (!occupiedCells.Contains(
                        rightCell))
                {
                    AddSideBoundary(
                        room,
                        cell.Col,
                        cell.Row,
                        Direction.Right,
                        activeDoors,
                        boundaryThickness);
                }

                var topCell =
                    (Col: cell.Col, Row: cell.Row - 1);

                if (!occupiedCells.Contains(
                        topCell))
                {
                    AddTopBoundary(
                        room,
                        cell.Col,
                        cell.Row,
                        activeDoors,
                        boundaryThickness);
                }
            }
        }

        // Создаёт боковую стену и оставляет проём только у активной боковой двери
        private static void AddSideBoundary(
            RoomTemplate room,
            int cellCol,
            int cellRow,
            Direction direction,
            IReadOnlyCollection<DoorSlot> activeDoors,
            double boundaryThickness)
        {
            DoorSlot? door =
                activeDoors.FirstOrDefault(
                    door =>
                        door.Direction ==
                            direction &&
                        door.CellCol ==
                            cellCol &&
                        door.CellRow ==
                            cellRow);

            double cellX =
                cellCol *
                RoomMetrics.CellWidth;

            double cellY =
                cellRow *
                RoomMetrics.CellHeight;

            double wallX =
                direction ==
                Direction.Left
                    ? cellX
                    : cellX +
                      RoomMetrics.CellWidth -
                      boundaryThickness;

            if (door is null)
            {
                room.Tiles.Add(
                    new TileData(
                        TileType.Ground,
                        wallX,
                        cellY,
                        boundaryThickness,
                        RoomMetrics.CellHeight));

                return;
            }

            double doorStartY =
                cellY +
                RoomMetrics.FloorY -
                RoomMetrics.SideDoorHeight;

            double doorEndY =
                cellY +
                RoomMetrics.FloorY;

            if (doorStartY >
                cellY)
            {
                room.Tiles.Add(
                    new TileData(
                        TileType.Ground,
                        wallX,
                        cellY,
                        boundaryThickness,
                        doorStartY -
                            cellY));
            }

            double cellBottom =
                cellY +
                RoomMetrics.CellHeight;

            if (doorEndY <
                cellBottom)
            {
                room.Tiles.Add(
                    new TileData(
                        TileType.Ground,
                        wallX,
                        doorEndY,
                        boundaryThickness,
                        cellBottom -
                            doorEndY));
            }
        }

        // Создаёт потолок и вырезает стандартный проход у активной верхней двери
        private static void AddTopBoundary(
            RoomTemplate room,
            int cellCol,
            int cellRow,
            IReadOnlyCollection<DoorSlot> activeDoors,
            double boundaryThickness)
        {
            DoorSlot? topDoor =
                activeDoors.FirstOrDefault(
                    door =>
                        door.Direction ==
                            Direction.Top &&
                        door.CellCol ==
                            cellCol &&
                        door.CellRow ==
                            cellRow);

            double cellX =
                cellCol *
                RoomMetrics.CellWidth;

            double cellY =
                cellRow *
                RoomMetrics.CellHeight;

            if (topDoor is null)
            {
                room.Tiles.Add(
                    new TileData(
                        TileType.Ground,
                        cellX,
                        cellY,
                        RoomMetrics.CellWidth,
                        boundaryThickness));

                return;
            }

            room.Tiles.Add(
                new TileData(
                    TileType.Ground,
                    cellX,
                    cellY,
                    RoomMetrics.TopBottomDoorStartX,
                    boundaryThickness));

            room.Tiles.Add(
                new TileData(
                    TileType.Ground,
                    cellX +
                        RoomMetrics.TopBottomDoorEndX,
                    cellY,
                    RoomMetrics.CellWidth -
                        RoomMetrics.TopBottomDoorEndX,
                    boundaryThickness));
        }

        // Добавляет лестницы к вертикальным переходам и этажам высокой комнаты
        private void AddVerticalAccess(
            RoomTemplate room,
            IReadOnlyCollection<DoorSlot> activeDoors)
        {
            var ladderColumns =
                activeDoors
                    .Where(
                        door =>
                            door.Direction is
                                Direction.Top or
                                Direction.Bottom)
                    .Select(
                        door =>
                            door.CellCol)
                    .ToHashSet();

            if (style ==
                GeneratedRoomStyle.Tall)
            {
                ladderColumns.Add(
                    0);
            }

            double ladderHeight =
                room.Height -
                RoomMetrics.FloorHeight;

            foreach (int cellCol in
                     ladderColumns)
            {
                room.Tiles.Add(
                    new TileData(
                        TileType.Ladder,
                        RoomLayoutRules.GetCenteredLadderX(
                            cellCol),
                        0,
                        RoomLayoutRules.LadderWidth,
                        ladderHeight));
            }

            AddBottomDoorLadderExtensions(
                room,
                activeDoors,
                ladderHeight);
        }

        // Продлевает лестницу под платформу нижнего прохода
        private static void AddBottomDoorLadderExtensions(
            RoomTemplate room,
            IReadOnlyCollection<DoorSlot> activeDoors,
            double mainLadderEndY)
        {
            foreach (var door in
                     activeDoors.Where(
                         door =>
                             door.Direction ==
                             Direction.Bottom))
            {
                double doorBoundaryY =
                    (door.CellRow + 1) *
                    RoomMetrics.CellHeight;

                if (doorBoundaryY <=
                    mainLadderEndY)
                {
                    continue;
                }

                room.Tiles.Add(
                    new TileData(
                        TileType.Ladder,
                        RoomLayoutRules.GetCenteredLadderX(
                            door.CellCol),
                        mainLadderEndY,
                        RoomLayoutRules.LadderWidth,
                        doorBoundaryY -
                            mainLadderEndY));
            }
        }

        // Добавляет площадки у потенциальных боковых дверей верхних блоков
        private void AddPotentialSideDoorPlatforms(
            RoomTemplate room)
        {
            const double platformWidth =
                260;

            const double platformHeight =
                20;

            var occupiedCells =
                OccupiedCells.ToHashSet();

            foreach (var door in
                     PotentialDoors.Where(
                         door =>
                             door.Direction is
                                 Direction.Left or
                                 Direction.Right))
            {
                var cellBelow = (
                    Col: door.CellCol,
                    Row: door.CellRow + 1);

                // На нижней внешней стороне уже существует обычный пол
                if (!occupiedCells.Contains(
                        cellBelow))
                {
                    continue;
                }

                double cellX =
                    door.CellCol *
                    RoomMetrics.CellWidth;

                double platformX =
                    door.Direction ==
                    Direction.Left
                        ? cellX
                        : cellX +
                          RoomMetrics.CellWidth -
                          platformWidth;

                double platformY =
                    door.CellRow *
                    RoomMetrics.CellHeight +
                    RoomMetrics.FloorY;

                room.Tiles.Add(
                    new TileData(
                        TileType.Platform,
                        platformX,
                        platformY,
                        platformWidth,
                        platformHeight));
            }
        }

        // Добавляет простое внутреннее наполнение выбранного типа комнаты
        private void AddDecorations(
            RoomTemplate room)
        {
            switch (style)
            {
                case GeneratedRoomStyle.Compact:
                    AddCompactDecorations(
                        room);
                    break;

                case GeneratedRoomStyle.Wide:
                    AddWideDecorations(
                        room);
                    break;

                case GeneratedRoomStyle.Tall:
                    AddTallDecorations(
                        room);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(style));
            }
        }

        // Добавляет две платформы в комнате один на один блок
        private static void AddCompactDecorations(
            RoomTemplate room)
        {
            room.Tiles.Add(
                new TileData(
                    TileType.Platform,
                    120,
                    RoomMetrics.FloorY - 150,
                    220,
                    20));

            room.Tiles.Add(
                new TileData(
                    TileType.Platform,
                    620,
                    RoomMetrics.FloorY - 230,
                    220,
                    20));
        }

        // Распределяет платформы между двумя горизонтальными блоками
        private static void AddWideDecorations(
            RoomTemplate room)
        {
            room.Tiles.Add(
                new TileData(
                    TileType.Platform,
                    180,
                    RoomMetrics.FloorY - 150,
                    240,
                    20));

            room.Tiles.Add(
                new TileData(
                    TileType.Platform,
                    1080,
                    RoomMetrics.FloorY - 220,
                    240,
                    20));

            room.Tiles.Add(
                new TileData(
                    TileType.Platform,
                    1580,
                    RoomMetrics.FloorY - 130,
                    180,
                    20));
        }

        // Добавляет платформы на верхнем и нижнем этажах высокой комнаты
        private static void AddTallDecorations(
            RoomTemplate room)
        {
            room.Tiles.Add(
                new TileData(
                    TileType.Platform,
                    100,
                    330,
                    250,
                    20));

            room.Tiles.Add(
                new TileData(
                    TileType.Platform,
                    610,
                    260,
                    250,
                    20));

            room.Tiles.Add(
                new TileData(
                    TileType.Platform,
                    120,
                    780,
                    240,
                    20));

            room.Tiles.Add(
                new TileData(
                    TileType.Platform,
                    610,
                    700,
                    240,
                    20));
        }

        // Добавляет небольшой Ground-элемент для проверки всех сторон твёрдой коллизии
        private void AddSolidCollisionSample(
            RoomTemplate room)
        {
            switch (style)
            {
                case GeneratedRoomStyle.Compact:
                    room.Tiles.Add(
                        new TileData(
                            TileType.Ground,
                            350,
                            300,
                            80,
                            30));
                    break;

                case GeneratedRoomStyle.Wide:
                    room.Tiles.Add(
                        new TileData(
                            TileType.Ground,
                            880,
                            320,
                            100,
                            30));
                    break;

                case GeneratedRoomStyle.Tall:
                    room.Tiles.Add(
                        new TileData(
                            TileType.Ground,
                            700,
                            850,
                            120,
                            30));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(style));
            }
        }
    }

    // Выбирает схему внутреннего наполнения комнаты
    public enum GeneratedRoomStyle
    {
        Compact,
        Wide,
        Tall
    }
}
