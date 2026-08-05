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
            double transitionSpeed = 60)
        {
            this.viewportWidth = viewportWidth;
            this.viewportHeight = viewportHeight;
            this.deadZoneWidth = deadZoneWidth;
            this.deadZoneHeight = deadZoneHeight;
            this.transitionSpeed = transitionSpeed;
        }

        // Двигает камеру, когда центр игрока покидает мёртвую зону
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

        // Быстро, но плавно вводит камеру в границы новой комнаты
        public bool MoveIntoBounds(
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

            double targetX = Math.Clamp(
                X,
                worldBounds.Left,
                maxX);

            double targetY = Math.Clamp(
                Y,
                worldBounds.Top,
                maxY);

            X = MoveTowards(
                X,
                targetX,
                transitionSpeed);

            Y = MoveTowards(
                Y,
                targetY,
                transitionSpeed);

            return Math.Abs(X - targetX) < 0.01 &&
                   Math.Abs(Y - targetY) < 0.01;
        }

        // Используется только для начального положения камеры
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

            Clamp(worldBounds);
        }

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
