using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WPFGame.Core;

namespace WPFGame.Level
{
    // Показывает весь LevelLayout и положение игрока для отладки генерации
    public sealed class DebugMiniMap
    {
        private const double PanelWidth =
            260;

        private const double PanelHeight =
            180;

        private const double HeaderHeight =
            24;

        private const double MapPadding =
            10;

        private readonly LevelLayout level;
        private readonly Canvas mapCanvas;
        private readonly TextBlock currentRoomText;
        private readonly Ellipse playerMarker;

        private readonly Dictionary<
            string,
            List<Rectangle>> roomCells =
                new();

        private readonly Brush normalRoomBrush =
            new SolidColorBrush(
                Color.FromRgb(
                    58,
                    64,
                    72));

        private readonly Brush currentRoomBrush =
            new SolidColorBrush(
                Color.FromRgb(
                    180,
                    120,
                    35));

        private double mapScale;
        private double mapOffsetX;
        private double mapOffsetY;

        private string? highlightedRoomId;

        public DebugMiniMap(
            Canvas viewport,
            LevelLayout level)
        {
            ArgumentNullException.ThrowIfNull(
                viewport);

            this.level =
                level ??
                throw new ArgumentNullException(
                    nameof(level));

            var border =
                CreateBorder();

            var grid =
                CreateLayoutGrid();

            currentRoomText =
                CreateHeader();

            mapCanvas =
                CreateMapCanvas();

            grid.Children.Add(
                currentRoomText);

            Grid.SetRow(
                mapCanvas,
                1);

            grid.Children.Add(
                mapCanvas);

            border.Child =
                grid;

            Canvas.SetLeft(
                border,
                viewport.Width -
                PanelWidth -
                12);

            Canvas.SetTop(
                border,
                12);

            Panel.SetZIndex(
                border,
                ZLayer.Interface +
                10);

            viewport.Children.Add(
                border);

            CalculateMapTransform();
            DrawRooms();
            DrawConnectionsAndDoors();
            DrawRoomLabels();

            playerMarker =
                CreatePlayerMarker();

            mapCanvas.Children.Add(
                playerMarker);

            PrintLayoutDiagnostics();
        }

        // Обновляет выделение текущей комнаты и точку игрока
        public void Update(
            RoomInstance currentRoom,
            double playerX,
            double playerY,
            double playerWidth,
            double playerHeight)
        {
            ArgumentNullException.ThrowIfNull(
                currentRoom);

            HighlightCurrentRoom(
                currentRoom.Id);

            currentRoomText.Text =
                $"MAP  |  {currentRoom.Id}";

            double playerCenterX =
                playerX +
                playerWidth / 2;

            double playerCenterY =
                playerY +
                playerHeight / 2;

            double markerX =
                WorldToMapX(
                    playerCenterX);

            double markerY =
                WorldToMapY(
                    playerCenterY);

            Canvas.SetLeft(
                playerMarker,
                markerX -
                playerMarker.Width / 2);

            Canvas.SetTop(
                playerMarker,
                markerY -
                playerMarker.Height / 2);
        }

        // Создаёт рамку, которая не зависит от CameraTransform
        private static Border CreateBorder()
        {
            return new Border
            {
                Width =
                    PanelWidth,

                Height =
                    PanelHeight,

                Background =
                    new SolidColorBrush(
                        Color.FromArgb(
                            220,
                            15,
                            17,
                            21)),

                BorderBrush =
                    Brushes.White,

                BorderThickness =
                    new Thickness(
                        1),

                CornerRadius =
                    new CornerRadius(
                        5)
            };
        }

        // Делит миникарту на заголовок и область схемы уровня
        private static Grid CreateLayoutGrid()
        {
            var grid =
                new Grid();

            grid.RowDefinitions.Add(
                new RowDefinition
                {
                    Height =
                        new GridLength(
                            HeaderHeight)
                });

            grid.RowDefinitions.Add(
                new RowDefinition
                {
                    Height =
                        new GridLength(
                            PanelHeight -
                            HeaderHeight)
                });

            return grid;
        }

        // Показывает ID текущего экземпляра комнаты
        private static TextBlock CreateHeader()
        {
            return new TextBlock
            {
                Text =
                    "MAP",

                Foreground =
                    Brushes.White,

                FontSize =
                    12,

                FontWeight =
                    FontWeights.Bold,

                Margin =
                    new Thickness(
                        8,
                        3,
                        0,
                        0)
            };
        }

        // Создаёт отдельный Canvas внутри рамки
        private static Canvas CreateMapCanvas()
        {
            return new Canvas
            {
                Width =
                    PanelWidth -
                    2,

                Height =
                    PanelHeight -
                    HeaderHeight -
                    2,

                ClipToBounds =
                    true
            };
        }

        // Рассчитывает общий масштаб, чтобы весь уровень сразу помещался в рамку
        private void CalculateMapTransform()
        {
            var occupiedCells =
                level.Rooms
                    .SelectMany(
                        room =>
                            room.GetOccupiedWorldCells())
                    .ToList();

            if (occupiedCells.Count == 0)
            {
                throw new InvalidOperationException(
                    "Миникарта не может отобразить пустой уровень.");
            }

            double worldMinX =
                occupiedCells.Min(
                    cell => cell.Col) *
                RoomMetrics.CellWidth;

            double worldMaxX =
                (occupiedCells.Max(
                    cell => cell.Col) + 1) *
                RoomMetrics.CellWidth;

            double worldMinY =
                occupiedCells.Min(
                    cell => cell.Row) *
                RoomMetrics.CellHeight;

            double worldMaxY =
                (occupiedCells.Max(
                    cell => cell.Row) + 1) *
                RoomMetrics.CellHeight;

            double worldWidth =
                worldMaxX -
                worldMinX;

            double worldHeight =
                worldMaxY -
                worldMinY;

            double availableWidth =
                mapCanvas.Width -
                MapPadding * 2;

            double availableHeight =
                mapCanvas.Height -
                MapPadding * 2;

            mapScale =
                Math.Min(
                    availableWidth /
                    worldWidth,

                    availableHeight /
                    worldHeight);

            double drawnWidth =
                worldWidth *
                mapScale;

            double drawnHeight =
                worldHeight *
                mapScale;

            mapOffsetX =
                (mapCanvas.Width -
                 drawnWidth) /
                2 -
                worldMinX *
                mapScale;

            mapOffsetY =
                (mapCanvas.Height -
                 drawnHeight) /
                2 -
                worldMinY *
                mapScale;
        }

        // Рисует каждый занятый блок каждого экземпляра комнаты
        private void DrawRooms()
        {
            foreach (var room in
                     level.Rooms)
            {
                var rectangles =
                    new List<Rectangle>();

                foreach (var cell in
                         room.GetOccupiedWorldCells())
                {
                    var rectangle =
                        new Rectangle
                        {
                            Width =
                                Math.Max(
                                    1,
                                    RoomMetrics.CellWidth *
                                    mapScale),

                            Height =
                                Math.Max(
                                    1,
                                    RoomMetrics.CellHeight *
                                    mapScale),

                            Fill =
                                normalRoomBrush,

                            Stroke =
                                Brushes.Gray,

                            StrokeThickness =
                                1
                        };

                    double worldX =
                        cell.Col *
                        RoomMetrics.CellWidth;

                    double worldY =
                        cell.Row *
                        RoomMetrics.CellHeight;

                    Canvas.SetLeft(
                        rectangle,
                        WorldToMapX(
                            worldX));

                    Canvas.SetTop(
                        rectangle,
                        WorldToMapY(
                            worldY));

                    mapCanvas.Children.Add(
                        rectangle);

                    rectangles.Add(
                        rectangle);
                }

                roomCells.Add(
                    room.Id,
                    rectangles);
            }
        }

        // Отмечает активные двери и отдельно показывает ошибочную стыковку
        private void DrawConnectionsAndDoors()
        {
            foreach (var room in
                     level.Rooms)
            {
                foreach (var door in
                         room.Template.Doors)
                {
                    Point center =
                        GetDoorWorldCenter(
                            room,
                            door);

                    bool connected =
                        level.GetConnectedRoom(
                            room.Id,
                            door.Id) is not null;

                    var doorMarker =
                        new Ellipse
                        {
                            Width =
                                4,

                            Height =
                                4,

                            Fill =
                                connected
                                    ? Brushes.LimeGreen
                                    : Brushes.OrangeRed
                        };

                    Canvas.SetLeft(
                        doorMarker,
                        WorldToMapX(
                            center.X) -
                        2);

                    Canvas.SetTop(
                        doorMarker,
                        WorldToMapY(
                            center.Y) -
                        2);

                    mapCanvas.Children.Add(
                        doorMarker);
                }
            }

            foreach (var connection in
                     level.Connections)
            {
                RoomInstance firstRoom =
                    level.GetRoom(
                        connection.First.RoomInstanceId);

                RoomInstance secondRoom =
                    level.GetRoom(
                        connection.Second.RoomInstanceId);

                DoorSlot firstDoor =
                    firstRoom.GetRequiredDoor(
                        connection.First.DoorId);

                DoorSlot secondDoor =
                    secondRoom.GetRequiredDoor(
                        connection.Second.DoorId);

                Point firstCenter =
                    GetDoorWorldCenter(
                        firstRoom,
                        firstDoor);

                Point secondCenter =
                    GetDoorWorldCenter(
                        secondRoom,
                        secondDoor);

                double deltaX =
                    secondCenter.X -
                    firstCenter.X;

                double deltaY =
                    secondCenter.Y -
                    firstCenter.Y;

                bool centersMatch =
                    Math.Abs(
                        deltaX) <
                        0.1 &&
                    Math.Abs(
                        deltaY) <
                        0.1;

                if (centersMatch)
                {
                    continue;
                }

                // Красная линия появляется только при несовпадении центров дверей
                var mismatchLine =
                    new Line
                    {
                        X1 =
                            WorldToMapX(
                                firstCenter.X),

                        Y1 =
                            WorldToMapY(
                                firstCenter.Y),

                        X2 =
                            WorldToMapX(
                                secondCenter.X),

                        Y2 =
                            WorldToMapY(
                                secondCenter.Y),

                        Stroke =
                            Brushes.Red,

                        StrokeThickness =
                            3
                    };

                mapCanvas.Children.Add(
                    mismatchLine);
            }
        }

        // Добавляет короткий номер экземпляра внутрь каждой комнаты
        private void DrawRoomLabels()
        {
            foreach (var room in
                     level.Rooms)
            {
                var cells =
                    room.GetOccupiedWorldCells()
                        .ToList();

                double centerX =
                    cells.Average(
                        cell =>
                            (cell.Col + 0.5) *
                            RoomMetrics.CellWidth);

                double centerY =
                    cells.Average(
                        cell =>
                            (cell.Row + 0.5) *
                            RoomMetrics.CellHeight);

                var label =
                    new TextBlock
                    {
                        Text =
                            GetShortRoomId(
                                room.Id),

                        Foreground =
                            Brushes.White,

                        FontSize =
                            9,

                        FontWeight =
                            FontWeights.Bold,

                        IsHitTestVisible =
                            false
                    };

                Canvas.SetLeft(
                    label,
                    WorldToMapX(
                        centerX) -
                    7);

                Canvas.SetTop(
                    label,
                    WorldToMapY(
                        centerY) -
                    7);

                mapCanvas.Children.Add(
                    label);
            }
        }

        // Создаёт маркер мирового центра игрока
        private static Ellipse CreatePlayerMarker()
        {
            return new Ellipse
            {
                Width =
                    8,

                Height =
                    8,

                Fill =
                    Brushes.White,

                Stroke =
                    Brushes.Red,

                StrokeThickness =
                    2,

                IsHitTestVisible =
                    false
            };
        }

        // Меняет цвет только при фактическом переходе в другой RoomInstance
        private void HighlightCurrentRoom(
            string roomId)
        {
            if (highlightedRoomId ==
                roomId)
            {
                return;
            }

            if (highlightedRoomId is not null &&
                roomCells.TryGetValue(
                    highlightedRoomId,
                    out var oldCells))
            {
                foreach (var rectangle in
                         oldCells)
                {
                    rectangle.Fill =
                        normalRoomBrush;
                }
            }

            if (roomCells.TryGetValue(
                    roomId,
                    out var newCells))
            {
                foreach (var rectangle in
                         newCells)
                {
                    rectangle.Fill =
                        currentRoomBrush;
                }
            }

            highlightedRoomId =
                roomId;
        }

        // Возвращает мировой центр конкретного дверного проёма
        private static Point GetDoorWorldCenter(
            RoomInstance room,
            DoorSlot door)
        {
            return door.Direction switch
            {
                Direction.Left =>
                    new Point(
                        room.OriginX +
                        door.CellCol *
                        RoomMetrics.CellWidth,

                        room.OriginY +
                        door.CellRow *
                        RoomMetrics.CellHeight +
                        RoomMetrics.FloorY -
                        RoomMetrics.SideDoorHeight /
                        2),

                Direction.Right =>
                    new Point(
                        room.OriginX +
                        (door.CellCol + 1) *
                        RoomMetrics.CellWidth,

                        room.OriginY +
                        door.CellRow *
                        RoomMetrics.CellHeight +
                        RoomMetrics.FloorY -
                        RoomMetrics.SideDoorHeight /
                        2),

                Direction.Top =>
                    new Point(
                        room.OriginX +
                        door.CellCol *
                        RoomMetrics.CellWidth +
                        RoomMetrics.TopBottomDoorStartX +
                        RoomMetrics.TopBottomDoorWidth /
                        2,

                        room.OriginY +
                        door.CellRow *
                        RoomMetrics.CellHeight),

                Direction.Bottom =>
                    new Point(
                        room.OriginX +
                        door.CellCol *
                        RoomMetrics.CellWidth +
                        RoomMetrics.TopBottomDoorStartX +
                        RoomMetrics.TopBottomDoorWidth /
                        2,

                        room.OriginY +
                        (door.CellRow + 1) *
                        RoomMetrics.CellHeight),

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(
                            door.Direction))
            };
        }

        // Переводит мировую X-координату в координату Canvas миникарты
        private double WorldToMapX(
            double worldX)
        {
            return mapOffsetX +
                   worldX *
                   mapScale;
        }

        // Переводит мировую Y-координату в координату Canvas миникарты
        private double WorldToMapY(
            double worldY)
        {
            return mapOffsetY +
                   worldY *
                   mapScale;
        }

        // Оставляет в подписи только короткую часть ID экземпляра
        private static string GetShortRoomId(
            string roomId)
        {
            int separator =
                roomId.LastIndexOf(
                    '_');

            return separator >= 0 &&
                   separator <
                   roomId.Length - 1
                ? roomId[
                    (separator + 1)..]
                : roomId;
        }

        // Печатает фактическое размещение и точность соединений в Output Debug
        private void PrintLayoutDiagnostics()
        {
            Debug.WriteLine(
                "========================================");

            Debug.WriteLine(
                $"[MINIMAP] Rooms={level.Rooms.Count}, " +
                $"Connections={level.Connections.Count}");

            foreach (var room in
                     level.Rooms.OrderBy(
                         room => room.Id))
            {
                string occupied =
                    string.Join(
                        ", ",
                        room.GetOccupiedWorldCells()
                            .Select(
                                cell =>
                                    $"({cell.Col},{cell.Row})"));

                Debug.WriteLine(
                    $"[MINIMAP] {room.Id} | " +
                    $"template={room.Template.Id} | " +
                    $"originCell=({room.WorldCellCol}," +
                    $"{room.WorldCellRow}) | " +
                    $"occupied={occupied}");
            }

            foreach (var connection in
                     level.Connections)
            {
                RoomInstance firstRoom =
                    level.GetRoom(
                        connection.First.RoomInstanceId);

                RoomInstance secondRoom =
                    level.GetRoom(
                        connection.Second.RoomInstanceId);

                DoorSlot firstDoor =
                    firstRoom.GetRequiredDoor(
                        connection.First.DoorId);

                DoorSlot secondDoor =
                    secondRoom.GetRequiredDoor(
                        connection.Second.DoorId);

                Point firstCenter =
                    GetDoorWorldCenter(
                        firstRoom,
                        firstDoor);

                Point secondCenter =
                    GetDoorWorldCenter(
                        secondRoom,
                        secondDoor);

                bool aligned =
                    RoomPlacement.AreDoorsAligned(
                        firstRoom,
                        firstDoor,
                        secondRoom,
                        secondDoor);

                double deltaX =
                    secondCenter.X -
                    firstCenter.X;

                double deltaY =
                    secondCenter.Y -
                    firstCenter.Y;

                Debug.WriteLine(
                    $"[MINIMAP] CONNECT " +
                    $"{connection.First.RoomInstanceId}/" +
                    $"{connection.First.DoorId} <-> " +
                    $"{connection.Second.RoomInstanceId}/" +
                    $"{connection.Second.DoorId} | " +
                    $"aligned={aligned} | " +
                    $"delta=({deltaX},{deltaY})");
            }

            Debug.WriteLine(
                "========================================");
        }
    }
}
