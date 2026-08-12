using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WPFGame.Core;

namespace WPFGame.Level
{
    // Отображает схему всего уровня и положение игрока
    public sealed class MiniMap
    {
        private const double PanelWidth = 260;
        private const double PanelHeight = 180;
        private const double HeaderHeight = 24;
        private const double MapPadding = 10;

        private const double RoomOutlineThickness = 1;
        private const double DoorMarkerThickness = 1.5;
        private const double DoorMarkerOpacity = 0.55;
        private const double DoorMarkerInset = 2.5;
        private const double DoorMarkerEndPadding = 2;

        private readonly LevelLayout level;
        private readonly Canvas mapCanvas;
        private readonly Ellipse playerMarker;

        private readonly Dictionary<
            string,
            List<Rectangle>> roomCells = new();

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

        public MiniMap(
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

            var header =
                CreateHeader();

            mapCanvas =
                CreateMapCanvas();

            grid.Children.Add(
                header);

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
                ZLayer.Interface + 10);

            viewport.Children.Add(
                border);

            CalculateMapTransform();
            DrawRooms();
            DrawDoors();

            playerMarker =
                CreatePlayerMarker();

            mapCanvas.Children.Add(
                playerMarker);
        }

        // Обновляет текущую комнату и положение маркера игрока
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

            double playerCenterX =
                playerX +
                playerWidth / 2;

            double playerCenterY =
                playerY +
                playerHeight / 2;

            Canvas.SetLeft(
                playerMarker,
                WorldToMapX(
                    playerCenterX) -
                playerMarker.Width / 2);

            Canvas.SetTop(
                playerMarker,
                WorldToMapY(
                    playerCenterY) -
                playerMarker.Height / 2);
        }

        // Создаёт внешнюю рамку миникарты
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
                        5),

                IsHitTestVisible =
                    false
            };
        }

        // Делит миникарту на заголовок и область уровня
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

        // Создаёт заголовок миникарты
        private static TextBlock CreateHeader()
        {
            return new TextBlock
            {
                Text =
                    "Карта",

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

        // Создаёт область для схемы уровня
        private static Canvas CreateMapCanvas()
        {
            return new Canvas
            {
                Width =
                    PanelWidth - 2,

                Height =
                    PanelHeight -
                    HeaderHeight -
                    2,

                ClipToBounds =
                    true
            };
        }

        // Выбирает единый масштаб, чтобы весь уровень помещался в миникарту
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

        // Рисует комнату как единую область без линий между её соседними блоками
        private void DrawRooms()
        {
            foreach (var room in
                     level.Rooms)
            {
                var rectangles =
                    new List<Rectangle>();

                var occupiedCells =
                    room.GetOccupiedWorldCells()
                        .ToList();

                foreach (var cell in
                         occupiedCells)
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

                            IsHitTestVisible =
                                false
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

                DrawRoomOutline(
                    occupiedCells);

                roomCells.Add(
                    room.Id,
                    rectangles);
            }
        }

        // Рисует только те стороны блоков, которые выходят наружу комнаты
        private void DrawRoomOutline(
            IReadOnlyCollection<(int Col, int Row)> occupiedCells)
        {
            var occupiedSet =
                occupiedCells.ToHashSet();

            foreach (var cell in
                     occupiedCells)
            {
                double left =
                    WorldToMapX(
                        cell.Col *
                        RoomMetrics.CellWidth);

                double right =
                    WorldToMapX(
                        (cell.Col + 1) *
                        RoomMetrics.CellWidth);

                double top =
                    WorldToMapY(
                        cell.Row *
                        RoomMetrics.CellHeight);

                double bottom =
                    WorldToMapY(
                        (cell.Row + 1) *
                        RoomMetrics.CellHeight);

                if (!occupiedSet.Contains(
                        (cell.Col - 1, cell.Row)))
                {
                    AddRoomOutlineLine(
                        left,
                        top,
                        left,
                        bottom);
                }

                if (!occupiedSet.Contains(
                        (cell.Col + 1, cell.Row)))
                {
                    AddRoomOutlineLine(
                        right,
                        top,
                        right,
                        bottom);
                }

                if (!occupiedSet.Contains(
                        (cell.Col, cell.Row - 1)))
                {
                    AddRoomOutlineLine(
                        left,
                        top,
                        right,
                        top);
                }

                if (!occupiedSet.Contains(
                        (cell.Col, cell.Row + 1)))
                {
                    AddRoomOutlineLine(
                        left,
                        bottom,
                        right,
                        bottom);
                }
            }
        }

        // Добавляет один внешний отрезок контура комнаты
        private void AddRoomOutlineLine(
            double x1,
            double y1,
            double x2,
            double y2)
        {
            var line =
                new Line
                {
                    X1 =
                        x1,

                    Y1 =
                        y1,

                    X2 =
                        x2,

                    Y2 =
                        y2,

                    Stroke =
                        Brushes.Gray,

                    StrokeThickness =
                        RoomOutlineThickness,

                    IsHitTestVisible =
                        false
                };

            mapCanvas.Children.Add(
                line);
        }

        // Помечает сторону конкретного блока, на которой находится активная дверь
        private void DrawDoors()
        {
            foreach (var room in
                     level.Rooms)
            {
                foreach (var door in
                         room.Template.Doors)
                {
                    AddDoorSideMarker(
                        room,
                        door);
                }
            }
        }

        // Рисует полупрозрачную линию вдоль стены блока с дверью
        private void AddDoorSideMarker(
            RoomInstance room,
            DoorSlot door)
        {
            double cellWorldX =
                room.OriginX +
                door.CellCol *
                RoomMetrics.CellWidth;

            double cellWorldY =
                room.OriginY +
                door.CellRow *
                RoomMetrics.CellHeight;

            double left =
                WorldToMapX(
                    cellWorldX);

            double right =
                WorldToMapX(
                    cellWorldX +
                    RoomMetrics.CellWidth);

            double top =
                WorldToMapY(
                    cellWorldY);

            double bottom =
                WorldToMapY(
                    cellWorldY +
                    RoomMetrics.CellHeight);

            var marker =
                new Line
                {
                    Stroke =
                        Brushes.LightGray,

                    StrokeThickness =
                        DoorMarkerThickness,

                    Opacity =
                        DoorMarkerOpacity,

                    IsHitTestVisible =
                        false
                };

            switch (door.Direction)
            {
                case Direction.Left:
                    marker.X1 =
                        left +
                        DoorMarkerInset;

                    marker.X2 =
                        marker.X1;

                    marker.Y1 =
                        top +
                        DoorMarkerEndPadding;

                    marker.Y2 =
                        bottom -
                        DoorMarkerEndPadding;
                    break;

                case Direction.Right:
                    marker.X1 =
                        right -
                        DoorMarkerInset;

                    marker.X2 =
                        marker.X1;

                    marker.Y1 =
                        top +
                        DoorMarkerEndPadding;

                    marker.Y2 =
                        bottom -
                        DoorMarkerEndPadding;
                    break;

                case Direction.Top:
                    marker.X1 =
                        left +
                        DoorMarkerEndPadding;

                    marker.X2 =
                        right -
                        DoorMarkerEndPadding;

                    marker.Y1 =
                        top +
                        DoorMarkerInset;

                    marker.Y2 =
                        marker.Y1;
                    break;

                case Direction.Bottom:
                    marker.X1 =
                        left +
                        DoorMarkerEndPadding;

                    marker.X2 =
                        right -
                        DoorMarkerEndPadding;

                    marker.Y1 =
                        bottom -
                        DoorMarkerInset;

                    marker.Y2 =
                        marker.Y1;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(
                            door.Direction));
            }

            mapCanvas.Children.Add(
                marker);
        }

        // Создаёт маркер центра игрока
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

        // Выделяет комнату, в которой сейчас находится игрок
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

        // Переводит мировую X-координату в координату миникарты
        private double WorldToMapX(
            double worldX)
        {
            return mapOffsetX +
                   worldX *
                   mapScale;
        }

        // Переводит мировую Y-координату в координату миникарты
        private double WorldToMapY(
            double worldY)
        {
            return mapOffsetY +
                   worldY *
                   mapScale;
        }
    }
}
