using System.Windows;

namespace WPFGame.Level
{
    // Следует за игроком и не выходит за мировые границы текущей комнаты
    public class CameraController
    {
        private readonly double viewportWidth;
        private readonly double viewportHeight;
        private readonly double deadZoneWidth;
        private readonly double deadZoneHeight;

        public double X { get; private set; }
        public double Y { get; private set; }

        public CameraController(
            double viewportWidth,
            double viewportHeight,
            double deadZoneWidth,
            double deadZoneHeight)
        {
            this.viewportWidth =
                viewportWidth;

            this.viewportHeight =
                viewportHeight;

            this.deadZoneWidth =
                deadZoneWidth;

            this.deadZoneHeight =
                deadZoneHeight;
        }

        // Двигает камеру после выхода игрока из мёртвой зоны
        public void Follow(
            double playerX,
            double playerY,
            double playerWidth,
            double playerHeight,
            Rect worldBounds)
        {
            double playerCenterX =
                playerX +
                playerWidth / 2;

            double playerCenterY =
                playerY +
                playerHeight / 2;

            double viewCenterX =
                X +
                viewportWidth / 2;

            double viewCenterY =
                Y +
                viewportHeight / 2;

            double left =
                viewCenterX -
                deadZoneWidth / 2;

            double right =
                viewCenterX +
                deadZoneWidth / 2;

            double top =
                viewCenterY -
                deadZoneHeight / 2;

            double bottom =
                viewCenterY +
                deadZoneHeight / 2;

            if (playerCenterX < left)
            {
                X -=
                    left -
                    playerCenterX;
            }
            else if (playerCenterX > right)
            {
                X +=
                    playerCenterX -
                    right;
            }

            if (playerCenterY < top)
            {
                Y -=
                    top -
                    playerCenterY;
            }
            else if (playerCenterY > bottom)
            {
                Y +=
                    playerCenterY -
                    bottom;
            }

            Clamp(
                worldBounds);
        }

        // Мгновенно центрирует камеру на игроке и ограничивает её границами комнаты
        public void SnapTo(
            double playerX,
            double playerY,
            double playerWidth,
            double playerHeight,
            Rect worldBounds)
        {
            X =
                playerX +
                playerWidth / 2 -
                viewportWidth / 2;

            Y =
                playerY +
                playerHeight / 2 -
                viewportHeight / 2;

            Clamp(
                worldBounds);
        }

        // Не позволяет области просмотра выйти за границы комнаты
        private void Clamp(
            Rect worldBounds)
        {
            double maxX =
                Math.Max(
                    worldBounds.Left,
                    worldBounds.Right -
                    viewportWidth);

            double maxY =
                Math.Max(
                    worldBounds.Top,
                    worldBounds.Bottom -
                    viewportHeight);

            X =
                Math.Clamp(
                    X,
                    worldBounds.Left,
                    maxX);

            Y =
                Math.Clamp(
                    Y,
                    worldBounds.Top,
                    maxY);
        }
    }
}
