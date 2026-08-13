using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WPFGame.Core;
using WPFGame.Level;

namespace WPFGame.GameFlow
{
    // Фиксированная небольшая сцена поезда между процедурными станциями
    public sealed class TrainScene
    {
        private const double WallThickness =
            30;

        private const double DepartureZoneWidth =
            60;

        private const double DepartureZoneHeight =
            70;

        private readonly List<Rectangle> renderedTiles =
            new();

        private Rectangle? departureMarker;

        public Rect Bounds { get; }

        public Point PlayerSpawn { get; }

        public InteractionZone DepartureZone { get; }

        public TrainScene(
            double holdDuration)
        {
            Bounds =
                new Rect(
                    0,
                    0,
                    RoomMetrics.CellWidth,
                    RoomMetrics.CellHeight);

            PlayerSpawn =
                new Point(
                    110,
                    RoomMetrics.FloorY -
                    RoomMetrics.DefaultPlayerHeight);

            Rect departureBounds =
                new Rect(
                    RoomMetrics.CellWidth -
                    WallThickness -
                    DepartureZoneWidth -
                    35,

                    RoomMetrics.FloorY -
                    DepartureZoneHeight,

                    DepartureZoneWidth,
                    DepartureZoneHeight);

            DepartureZone =
                new InteractionZone(
                    departureBounds,
                    "Удерживайте E, чтобы отправиться",
                    holdDuration);
        }

        // Добавляет фиксированную геометрию поезда на игровой Canvas
        public void Load(
            Canvas gameArea)
        {
            ArgumentNullException.ThrowIfNull(
                gameArea);

            if (renderedTiles.Count > 0)
            {
                return;
            }

            RoomTemplate room =
                CreateRoom();

            renderedTiles.AddRange(
                RoomRenderer.Render(
                    gameArea,
                    room));

            departureMarker =
                CreateDepartureMarker();

            gameArea.Children.Add(
                departureMarker);

            Canvas.SetLeft(
                departureMarker,
                DepartureZone.Bounds.Left);

            Canvas.SetTop(
                departureMarker,
                DepartureZone.Bounds.Top);

            Panel.SetZIndex(
                departureMarker,
                ZLayer.Tiles + 1);
        }

        // Удаляет все визуальные элементы поезда с игрового Canvas
        public void Unload(
            Canvas gameArea)
        {
            ArgumentNullException.ThrowIfNull(
                gameArea);

            foreach (Rectangle tile in
                     renderedTiles)
            {
                gameArea.Children.Remove(
                    tile);
            }

            renderedTiles.Clear();

            if (departureMarker is not null)
            {
                gameArea.Children.Remove(
                    departureMarker);

                departureMarker =
                    null;
            }

            DepartureZone.Reset();
        }

        // Создаёт одну фиксированную комнату без LevelLayout и генератора
        private static RoomTemplate CreateRoom()
        {
            var room =
                new RoomTemplate(
                    "train_room",
                    new[]
                    {
                        (Col: 0, Row: 0)
                    });

            room.PlayerStartX =
                110;

            room.PlayerStartY =
                RoomMetrics.FloorY -
                RoomMetrics.DefaultPlayerHeight;

            room.Tiles.Add(
                new TileData(
                    TileType.Ground,
                    0,
                    RoomMetrics.FloorY,
                    RoomMetrics.CellWidth,
                    RoomMetrics.FloorHeight));

            room.Tiles.Add(
                new TileData(
                    TileType.Ground,
                    0,
                    0,
                    WallThickness,
                    RoomMetrics.CellHeight));

            room.Tiles.Add(
                new TileData(
                    TileType.Ground,
                    RoomMetrics.CellWidth -
                    WallThickness,
                    0,
                    WallThickness,
                    RoomMetrics.CellHeight));

            room.Tiles.Add(
                new TileData(
                    TileType.Ground,
                    0,
                    0,
                    RoomMetrics.CellWidth,
                    WallThickness));

            return room;
        }

        // Создаёт временное визуальное обозначение зоны отправления
        private Rectangle CreateDepartureMarker()
        {
            return new Rectangle
            {
                Width =
                    DepartureZone.Bounds.Width,

                Height =
                    DepartureZone.Bounds.Height,

                Fill =
                    Brushes.Goldenrod,

                Stroke =
                    Brushes.White,

                StrokeThickness =
                    2,

                Opacity =
                    0.45,

                IsHitTestVisible =
                    false,

                Tag =
                    "InteractionMarker"
            };
        }

    }
}
