namespace WPFGame.Level
{
    // Создаёт связную цепочку комнат по заданному seed
    public static class LevelGenerator
    {
        private const int MaxGenerationAttempts =
            200;

        public static LevelLayout Generate(
            int seed,
            int roomCount = 8)
        {
            if (roomCount < 2)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(roomCount),
                    "Уровень должен содержать минимум две комнаты.");
            }

            if (roomCount > 20)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(roomCount),
                    "Первый генератор ограничен двадцатью комнатами.");
            }

            IReadOnlyList<
                GeneratedRoomDefinition> definitions =
                    GeneratedRoomCatalog.CreateDefault();

            for (int attempt = 0;
                 attempt < MaxGenerationAttempts;
                 attempt++)
            {
                int attemptSeed =
                    unchecked(
                        seed +
                        attempt *
                        104729);

                var random =
                    new Random(
                        attemptSeed);

                if (TryGenerate(
                        random,
                        roomCount,
                        definitions,
                        out LevelLayout? level))
                {
                    return level;
                }
            }

            throw new InvalidOperationException(
                $"Не удалось построить уровень из {roomCount} комнат " +
                $"для seed {seed}.");
        }

        // Выполняет одну попытку последовательного размещения комнат
        private static bool TryGenerate(
            Random random,
            int roomCount,
            IReadOnlyList<
                GeneratedRoomDefinition> definitions,
            out LevelLayout level)
        {
            level =
                new LevelLayout();

            var usedDirections =
                new HashSet<Direction>();

            var usedDefinitionIds =
                new HashSet<string>();

            GeneratedRoomDefinition startDefinition =
                definitions.First(
                    definition =>
                        definition.Id ==
                        "compact_1x1");

            DoorSlot startExit =
                startDefinition.PotentialDoors[
                    random.Next(
                        startDefinition.PotentialDoors.Count)];

            RoomTemplate startTemplate =
                startDefinition.CreateTemplate(
                    CreateTemplateId(
                        0,
                        startDefinition),
                    new[]
                    {
                        startExit
                    });

            var startRoom =
                new RoomInstance(
                    CreateInstanceId(
                        0),
                    startTemplate,
                    worldCellCol: 0,
                    worldCellRow: 0);

            level.AddRoom(
                startRoom,
                isStartRoom: true);

            usedDefinitionIds.Add(
                startDefinition.Id);

            RoomInstance sourceRoom =
                startRoom;

            DoorSlot sourceExit =
                sourceRoom.GetRequiredDoor(
                    startExit.Id);

            for (int index = 1;
                 index < roomCount;
                 index++)
            {
                bool isLastRoom =
                    index ==
                    roomCount - 1;

                usedDirections.Add(
                    sourceExit.Direction);

                List<PlacementCandidate> candidates =
                    CreateCandidates(
                        level,
                        sourceRoom,
                        sourceExit,
                        index,
                        isLastRoom,
                        definitions);

                if (candidates.Count == 0)
                {
                    return false;
                }

                candidates =
                    PreferNewDirections(
                        candidates,
                        usedDirections,
                        isLastRoom);

                candidates =
                    PreferUnusedDefinitions(
                        candidates,
                        usedDefinitionIds);

                PlacementCandidate selected =
                    candidates[
                        random.Next(
                            candidates.Count)];

                var room =
                    new RoomInstance(
                        CreateInstanceId(
                            index),
                        selected.Template,
                        selected.WorldCellCol,
                        selected.WorldCellRow);

                level.AddRoom(
                    room);

                level.Connect(
                    sourceRoom.Id,
                    sourceExit.Id,
                    room.Id,
                    selected.EntryDoor.Id);

                usedDefinitionIds.Add(
                    selected.Definition.Id);

                if (isLastRoom)
                {
                    break;
                }

                if (selected.ExitDoor is null)
                {
                    return false;
                }

                sourceRoom =
                    room;

                sourceExit =
                    room.GetRequiredDoor(
                        selected.ExitDoor.Id);
            }

            return level.Rooms.Count ==
                   roomCount;
        }

        // Перебирает формы, входные двери, выходы и свободные позиции
        private static List<PlacementCandidate>
            CreateCandidates(
                LevelLayout level,
                RoomInstance sourceRoom,
                DoorSlot sourceExit,
                int roomIndex,
                bool isLastRoom,
                IReadOnlyList<
                    GeneratedRoomDefinition> definitions)
        {
            var result =
                new List<PlacementCandidate>();

            Direction requiredEntryDirection =
                sourceExit.Direction.Opposite();

            foreach (var definition in
                     definitions)
            {
                foreach (var entryDoor in
                         definition.GetPotentialDoors(
                             requiredEntryDirection))
                {
                    IEnumerable<DoorSlot?>
                        exitOptions =
                            isLastRoom
                                ? new DoorSlot?[]
                                {
                                    null
                                }
                                : definition.PotentialDoors
                                    .Where(
                                        door =>
                                            door.Id !=
                                            entryDoor.Id)
                                    .Select(
                                        door =>
                                            (DoorSlot?)door);

                    foreach (DoorSlot? exitDoor in
                             exitOptions)
                    {
                        var activeDoors =
                            new List<DoorSlot>
                            {
                                entryDoor
                            };

                        if (exitDoor is not null)
                        {
                            activeDoors.Add(
                                exitDoor);
                        }

                        RoomTemplate template =
                            definition.CreateTemplate(
                                CreateTemplateId(
                                    roomIndex,
                                    definition),
                                activeDoors);

                        var targetCell =
                            RoomPlacement.CalculateTargetCell(
                                sourceRoom,
                                sourceExit,
                                template,
                                entryDoor);

                        if (!level.CanPlaceRoom(
                                template,
                                targetCell.Col,
                                targetCell.Row))
                        {
                            continue;
                        }

                        result.Add(
                            new PlacementCandidate(
                                definition,
                                template,
                                entryDoor,
                                exitDoor,
                                targetCell.Col,
                                targetCell.Row));
                    }
                }
            }

            return result;
        }

        // При возможности выбирает ещё не использованное направление выхода
        private static List<PlacementCandidate>
            PreferNewDirections(
                List<PlacementCandidate> candidates,
                HashSet<Direction> usedDirections,
                bool isLastRoom)
        {
            if (isLastRoom)
            {
                return candidates;
            }

            var preferred =
                candidates
                    .Where(
                        candidate =>
                            candidate.ExitDoor is not null &&
                            !usedDirections.Contains(
                                candidate.ExitDoor.Direction))
                    .ToList();

            return preferred.Count > 0
                ? preferred
                : candidates;
        }

        // При возможности добавляет ещё не встречавшуюся форму комнаты
        private static List<PlacementCandidate>
            PreferUnusedDefinitions(
                List<PlacementCandidate> candidates,
                HashSet<string> usedDefinitionIds)
        {
            var preferred =
                candidates
                    .Where(
                        candidate =>
                            !usedDefinitionIds.Contains(
                                candidate.Definition.Id))
                    .ToList();

            return preferred.Count > 0
                ? preferred
                : candidates;
        }

        // Создаёт читаемый ID экземпляра по его порядковому номеру
        private static string CreateInstanceId(
            int index)
        {
            return $"generated_{index:00}";
        }

        // Создаёт уникальный ID шаблона конкретного экземпляра
        private static string CreateTemplateId(
            int index,
            GeneratedRoomDefinition definition)
        {
            return
                $"generated_template_{index:00}_{definition.Id}";
        }

        // Хранит один допустимый вариант следующей комнаты
        private sealed class PlacementCandidate
        {
            public GeneratedRoomDefinition Definition
            {
                get;
            }

            public RoomTemplate Template
            {
                get;
            }

            public DoorSlot EntryDoor
            {
                get;
            }

            public DoorSlot? ExitDoor
            {
                get;
            }

            public int WorldCellCol
            {
                get;
            }

            public int WorldCellRow
            {
                get;
            }

            public PlacementCandidate(
                GeneratedRoomDefinition definition,
                RoomTemplate template,
                DoorSlot entryDoor,
                DoorSlot? exitDoor,
                int worldCellCol,
                int worldCellRow)
            {
                Definition =
                    definition;

                Template =
                    template;

                EntryDoor =
                    entryDoor;

                ExitDoor =
                    exitDoor;

                WorldCellCol =
                    worldCellCol;

                WorldCellRow =
                    worldCellRow;
            }
        }
    }
}
