using System.Diagnostics;
using System.Text;

namespace WPFGame.Level
{
    // Формирует подробный отчёт о готовом сгенерированном уровне
    public static class LevelGenerationDiagnostics
    {
        public static void Print(
            LevelLayout level,
            int seed)
        {
            string report =
                BuildReport(
                    level,
                    seed);

            Debug.WriteLine(
                report);
        }

        // Собирает текстовый отчёт по комнатам, дверям и навигационным тайлам
        public static string BuildReport(
            LevelLayout level,
            int seed)
        {
            ArgumentNullException.ThrowIfNull(
                level);

            var builder =
                new StringBuilder();

            builder.AppendLine(
                "========================================");
            builder.AppendLine(
                $"LEVEL GENERATION DIAGNOSTICS | seed = {seed}");
            builder.AppendLine(
                $"Rooms = {level.Rooms.Count}, " +
                $"Connections = {level.Connections.Count}");
            builder.AppendLine(
                "========================================");

            var definitions =
                GeneratedRoomCatalog.CreateDefault();

            foreach (var room in
                     level.Rooms.OrderBy(
                         room => room.Id))
            {
                AppendRoom(
                    builder,
                    level,
                    room,
                    definitions);
            }

            builder.AppendLine(
                "========================================");

            return builder.ToString();
        }

        // Добавляет информацию об одном экземпляре комнаты
        private static void AppendRoom(
            StringBuilder builder,
            LevelLayout level,
            RoomInstance room,
            IReadOnlyList<
                GeneratedRoomDefinition> definitions)
        {
            GeneratedRoomDefinition? definition =
                definitions.FirstOrDefault(
                    definition =>
                        room.Template.Id.EndsWith(
                            definition.Id,
                            StringComparison.Ordinal));

            builder.AppendLine();
            builder.AppendLine(
                $"[{room.Id}]");
            builder.AppendLine(
                $"Template: {room.Template.Id}");
            builder.AppendLine(
                $"Definition: {definition?.Id ?? "unknown"}");
            builder.AppendLine(
                $"World cell: ({room.WorldCellCol}, {room.WorldCellRow})");
            builder.AppendLine(
                $"Origin: ({room.OriginX}, {room.OriginY})");
            builder.AppendLine(
                $"Size: {room.Width} x {room.Height}");

            builder.Append(
                "Occupied world cells: ");

            builder.AppendLine(
                string.Join(
                    ", ",
                    room.GetOccupiedWorldCells()
                        .Select(
                            cell =>
                                $"({cell.Col},{cell.Row})")));

            AppendDoors(
                builder,
                level,
                room);

            AppendTileSummary(
                builder,
                room);

            AppendLadders(
                builder,
                room);

            AppendPlatforms(
                builder,
                room);

            AppendBottomDoorChecks(
                builder,
                room);

            if (definition is not null)
            {
                AppendPotentialSideDoorChecks(
                    builder,
                    room,
                    definition);
            }
        }

        // Выводит активные двери и фактические связи уровня
        private static void AppendDoors(
            StringBuilder builder,
            LevelLayout level,
            RoomInstance room)
        {
            builder.AppendLine(
                "Active doors:");

            foreach (var door in
                     room.Template.Doors)
            {
                var connected =
                    level.GetConnectedRoom(
                        room.Id,
                        door.Id);

                string target =
                    connected is null
                        ? "not connected"
                        : $"{connected.Value.Room.Id}/" +
                          $"{connected.Value.Door.Id}";

                builder.AppendLine(
                    $"  {door.Id} | {door.Direction} | " +
                    $"cell=({door.CellCol},{door.CellRow}) | " +
                    $"-> {target}");
            }
        }

        // Выводит количество тайлов каждого типа
        private static void AppendTileSummary(
            StringBuilder builder,
            RoomInstance room)
        {
            var summary =
                room.Template.Tiles
                    .GroupBy(
                        tile => tile.Type)
                    .OrderBy(
                        group => group.Key.ToString())
                    .Select(
                        group =>
                            $"{group.Key}={group.Count()}");

            builder.AppendLine(
                "Tiles: " +
                string.Join(
                    ", ",
                    summary));
        }

        // Выводит точные размеры и координаты всех лестниц
        private static void AppendLadders(
            StringBuilder builder,
            RoomInstance room)
        {
            builder.AppendLine(
                "Ladders:");

            var ladders =
                room.Template.Tiles
                    .Where(
                        tile =>
                            tile.Type ==
                            TileType.Ladder)
                    .ToList();

            if (ladders.Count == 0)
            {
                builder.AppendLine(
                    "  none");

                return;
            }

            foreach (var ladder in
                     ladders)
            {
                builder.AppendLine(
                    $"  local=({ladder.X},{ladder.Y}) " +
                    $"size={ladder.Width}x{ladder.Height} | " +
                    $"world=({room.OriginX + ladder.X}," +
                    $"{room.OriginY + ladder.Y})");
            }
        }

        // Выводит точные размеры и координаты всех платформ
        private static void AppendPlatforms(
            StringBuilder builder,
            RoomInstance room)
        {
            builder.AppendLine(
                "Platforms:");

            var platforms =
                room.Template.Tiles
                    .Where(
                        tile =>
                            tile.Type ==
                            TileType.Platform)
                    .ToList();

            if (platforms.Count == 0)
            {
                builder.AppendLine(
                    "  none");

                return;
            }

            foreach (var platform in
                     platforms)
            {
                builder.AppendLine(
                    $"  local=({platform.X},{platform.Y}) " +
                    $"size={platform.Width}x{platform.Height}");
            }
        }

        // Проверяет, доходит ли лестница до нижней границы активной Bottom-двери
        private static void AppendBottomDoorChecks(
            StringBuilder builder,
            RoomInstance room)
        {
            foreach (var door in
                     room.Template.Doors.Where(
                         door =>
                             door.Direction ==
                             Direction.Bottom))
            {
                double expectedX =
                    RoomLayoutRules.GetCenteredLadderX(
                        door.CellCol);

                double boundaryY =
                    (door.CellRow + 1) *
                    RoomMetrics.CellHeight;

                bool hasLadderToBoundary =
                    room.Template.Tiles.Any(
                        tile =>
                            tile.Type ==
                                TileType.Ladder &&
                            Math.Abs(
                                tile.X -
                                expectedX) <
                                0.1 &&
                            tile.Y <=
                                boundaryY &&
                            tile.Y +
                                tile.Height >=
                                boundaryY);

                builder.AppendLine(
                    $"Bottom door check {door.Id}: " +
                    (hasLadderToBoundary
                        ? "OK - ladder reaches boundary"
                        : "MISSING - ladder does not reach boundary"));
            }
        }

        // Проверяет площадки у потенциальных боковых дверей верхних блоков
        private static void AppendPotentialSideDoorChecks(
            StringBuilder builder,
            RoomInstance room,
            GeneratedRoomDefinition definition)
        {
            var occupied =
                definition.OccupiedCells
                    .ToHashSet();

            foreach (var door in
                     definition.PotentialDoors.Where(
                         door =>
                             door.Direction is
                                 Direction.Left or
                                 Direction.Right))
            {
                var cellBelow = (
                    Col: door.CellCol,
                    Row: door.CellRow + 1);

                if (!occupied.Contains(
                        cellBelow))
                {
                    continue;
                }

                double platformY =
                    door.CellRow *
                    RoomMetrics.CellHeight +
                    RoomMetrics.FloorY;

                bool hasEdgePlatform =
                    room.Template.Tiles.Any(
                        tile =>
                            tile.Type ==
                                TileType.Platform &&
                            Math.Abs(
                                tile.Y -
                                platformY) <
                                0.1 &&
                            IsPlatformAtDoorEdge(
                                tile,
                                door));

                builder.AppendLine(
                    $"Potential side access {door.Id}: " +
                    (hasEdgePlatform
                        ? "OK - edge platform exists"
                        : "MISSING - no edge platform"));
            }
        }

        // Проверяет, касается ли платформа нужного края блока
        private static bool IsPlatformAtDoorEdge(
            TileData tile,
            DoorSlot door)
        {
            double cellLeft =
                door.CellCol *
                RoomMetrics.CellWidth;

            double cellRight =
                cellLeft +
                RoomMetrics.CellWidth;

            return door.Direction switch
            {
                Direction.Left =>
                    Math.Abs(
                        tile.X -
                        cellLeft) <
                    0.1,

                Direction.Right =>
                    Math.Abs(
                        tile.X +
                        tile.Width -
                        cellRight) <
                    0.1,

                _ => false
            };
        }
    }
}
