using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WPFGame.Core;
using WPFGame.Level;

namespace WPFGame.GameFlow
{
    // Представляет одну загруженную процедурную станцию
    public sealed class StationScene
    {
        private const double ExitWidth =
            40;

        private const double ExitHeight =
            70;

        private const double ExitOffsetX =
            50;

        private readonly Canvas gameArea;
        private readonly RoomManager roomManager;
        private readonly MiniMap miniMap;
        private readonly Rectangle exitMarker;

        public Point PlayerSpawn { get; }

        public InteractionZone ExitZone { get; }

        public Rect CurrentBounds =>
            roomManager.CurrentBounds;

        public Rect ActiveBounds =>
            roomManager.ActiveBounds;

        public bool CurrentRoomChanged =>
            roomManager.CurrentRoomChanged;

        public StationScene(
            Canvas gameArea,
            Canvas viewport,
            int levelSeed,
            int roomCount,
            double interactionHoldDuration)
        {
            ArgumentNullException.ThrowIfNull(
                gameArea);

            ArgumentNullException.ThrowIfNull(
                viewport);

            this.gameArea =
                gameArea;

            LevelLayout level =
                LevelGenerator.Generate(
                    levelSeed,
                    roomCount);

            roomManager =
                new RoomManager(
                    gameArea,
                    level);

            miniMap =
                new MiniMap(
                    viewport,
                    level);

            PlayerSpawn =
                new Point(
                    roomManager.CurrentOriginX +
                    roomManager.CurrentRoom.PlayerStartX,

                    roomManager.CurrentOriginY +
                    roomManager.CurrentRoom.PlayerStartY);

            ExitZone =
                CreateExitZone(
                    interactionHoldDuration);

            exitMarker =
                CreateExitMarker();

            gameArea.Children.Add(
                exitMarker);

            Canvas.SetLeft(
                exitMarker,
                ExitZone.Bounds.Left);

            Canvas.SetTop(
                exitMarker,
                ExitZone.Bounds.Top);

            Panel.SetZIndex(
                exitMarker,
                ZLayer.Tiles + 1);
        }

        // Обновляет переход между комнатами и возвращает допустимую позицию игрока
        public Point UpdatePlayer(
            double previousPlayerX,
            double previousPlayerY,
            double playerX,
            double playerY,
            double playerWidth,
            double playerHeight)
        {
            return roomManager.UpdatePlayer(
                previousPlayerX,
                previousPlayerY,
                playerX,
                playerY,
                playerWidth,
                playerHeight);
        }

        // Обновляет положение игрока и текущую комнату на миникарте
        public void UpdateMiniMap(
            double playerX,
            double playerY,
            double playerWidth,
            double playerHeight)
        {
            miniMap.Update(
                roomManager.CurrentInstance,
                playerX,
                playerY,
                playerWidth,
                playerHeight);
        }

        // Удаляет все визуальные элементы, принадлежащие станции
        public void Unload()
        {
            ExitZone.Reset();

            roomManager.Unload();
            miniMap.Remove();

            gameArea.Children.Remove(
                exitMarker);
        }

        // Создаёт точку возврата возле spawn стартовой комнаты
        private InteractionZone CreateExitZone(
            double interactionHoldDuration)
        {
            Rect bounds =
                new Rect(
                    roomManager.CurrentOriginX +
                    ExitOffsetX,

                    roomManager.CurrentOriginY +
                    RoomMetrics.FloorY -
                    ExitHeight,

                    ExitWidth,
                    ExitHeight);

            return new InteractionZone(
                bounds,
                "Удерживайте E, чтобы покинуть уровень",
                interactionHoldDuration);
        }

        // Создаёт визуальное обозначение зоны возврата
        private Rectangle CreateExitMarker()
        {
            return new Rectangle
            {
                Width =
                    ExitZone.Bounds.Width,

                Height =
                    ExitZone.Bounds.Height,

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
