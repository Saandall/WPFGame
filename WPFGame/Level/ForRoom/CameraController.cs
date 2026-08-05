using System.Windows;

namespace WPFGame.Level
{
    // Следует за игроком и не выходит за мировые границы комнаты
    public class CameraController
    {
        private readonly double viewportWidth;
        private readonly double viewportHeight;
        private readonly double deadZoneWidth;
        private readonly double deadZoneHeight;
        private readonly double transitionSpeed;

        public double X { get; private set; }
        public double Y { get; private set; }

        public CameraController(
            double viewportWidth,
            double viewportHeight,
            double deadZoneWidth,
            double deadZoneHeight,
            double transitionSpeed = 80)
        {
            this.viewportWidth = viewportWidth;
            this.viewportHeight = viewportHeight;
            this.deadZoneWidth = deadZoneWidth;
            this.deadZoneHeight = deadZoneHeight;
            this.transitionSpeed = transitionSpeed;
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
                playerX + playerWidth / 2;

            double playerCenterY =
                playerY + playerHeight / 2;

            double viewCenterX =
                X + viewportWidth / 2;

            double viewCenterY =
                Y + viewportHeight / 2;

            double left =
                viewCenterX - deadZoneWidth / 2;

            double right =
                viewCenterX + deadZoneWidth / 2;

            double top =
                viewCenterY - deadZoneHeight / 2;

            double bottom =
                viewCenterY + deadZoneHeight / 2;

            if (playerCenterX < left)
            {
                X -= left - playerCenterX;
            }
            else if (playerCenterX > right)
            {
                X += playerCenterX - right;
            }

            if (playerCenterY < top)
            {
                Y -= top - playerCenterY;
            }
            else if (playerCenterY > bottom)
            {
                Y += playerCenterY - bottom;
            }

            Clamp(worldBounds);
        }

        // Быстро перемещает камеру к игроку после перехода
        public bool MoveQuicklyToPlayer(
            double playerX,
            double playerY,
            double playerWidth,
            double playerHeight,
            Rect worldBounds)
        {
            Point target =
                GetCenteredPosition(
                    playerX,
                    playerY,
                    playerWidth,
                    playerHeight,
                    worldBounds);

            X = MoveTowards(
                X,
                target.X,
                transitionSpeed);

            Y = MoveTowards(
                Y,
                target.Y,
                transitionSpeed);

            return Math.Abs(X - target.X) < 0.01 &&
                   Math.Abs(Y - target.Y) < 0.01;
        }

        // Устанавливает начальное положение камеры
        public void SnapTo(
            double playerX,
            double playerY,
            double playerWidth,
            double playerHeight,
            Rect worldBounds)
        {
            Point target =
                GetCenteredPosition(
                    playerX,
                    playerY,
                    playerWidth,
                    playerHeight,
                    worldBounds);

            X = target.X;
            Y = target.Y;
        }

        // Рассчитывает положение камеры с учётом границ комнаты
        private Point GetCenteredPosition(
            double playerX,
            double playerY,
            double playerWidth,
            double playerHeight,
            Rect worldBounds)
        {
            double targetX =
                playerX +
                playerWidth / 2 -
                viewportWidth / 2;

            double targetY =
                playerY +
                playerHeight / 2 -
                viewportHeight / 2;

            double maxX = Math.Max(
                worldBounds.Left,
                worldBounds.Right -
                viewportWidth);

            double maxY = Math.Max(
                worldBounds.Top,
                worldBounds.Bottom -
                viewportHeight);

            return new Point(
                Math.Clamp(
                    targetX,
                    worldBounds.Left,
                    maxX),

                Math.Clamp(
                    targetY,
                    worldBounds.Top,
                    maxY));
        }

        // Приближает значение к цели с ограниченной скоростью
        private static double MoveTowards(
            double current,
            double target,
            double maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta)
            {
                return target;
            }

            return current +
                   Math.Sign(target - current) *
                   maxDelta;
        }

        // Ограничивает положение камеры границами комнаты
        private void Clamp(
            Rect worldBounds)
        {
            double maxX = Math.Max(
                worldBounds.Left,
                worldBounds.Right -
                viewportWidth);

            double maxY = Math.Max(
                worldBounds.Top,
                worldBounds.Bottom -
                viewportHeight);

            X = Math.Clamp(
                X,
                worldBounds.Left,
                maxX);

            Y = Math.Clamp(
                Y,
                worldBounds.Top,
                maxY);
        }
    }
}
