using System;

namespace WPFGame.Level
{
    // Какая часть комнаты сейчас видна. Следует за игроком, только если он вышел
    // за пределы "мёртвой зоны" в центре экрана, и не показывает то, что за границами комнаты.
    public class CameraController
    {
        private readonly double viewportWidth;
        private readonly double viewportHeight;
        private readonly double deadZoneWidth;
        private readonly double deadZoneHeight;

        public double X { get; private set; }
        public double Y { get; private set; }

        public CameraController(double viewportWidth, double viewportHeight, double deadZoneWidth, double deadZoneHeight)
        {
            this.viewportWidth = viewportWidth;
            this.viewportHeight = viewportHeight;
            this.deadZoneWidth = deadZoneWidth;
            this.deadZoneHeight = deadZoneHeight;
        }

        // Двигает камеру, только если центр игрока вышел за мёртвую зону
        public void Follow(double playerX, double playerY, double playerWidth, double playerHeight, double roomWidth, double roomHeight)
        {
            double playerCenterX = playerX + playerWidth / 2;
            double playerCenterY = playerY + playerHeight / 2;

            double viewCenterX = X + viewportWidth / 2;
            double viewCenterY = Y + viewportHeight / 2;

            double left = viewCenterX - deadZoneWidth / 2;
            double right = viewCenterX + deadZoneWidth / 2;
            double top = viewCenterY - deadZoneHeight / 2;
            double bottom = viewCenterY + deadZoneHeight / 2;

            if (playerCenterX < left) X -= left - playerCenterX;
            else if (playerCenterX > right) X += playerCenterX - right;

            if (playerCenterY < top) Y -= top - playerCenterY;
            else if (playerCenterY > bottom) Y += playerCenterY - bottom;

            Clamp(roomWidth, roomHeight);
        }

        // Мгновенно центрирует камеру на игроке — используем сразу после смены комнаты
        public void SnapTo(double playerX, double playerY, double playerWidth, double playerHeight, double roomWidth, double roomHeight)
        {
            X = playerX + playerWidth / 2 - viewportWidth / 2;
            Y = playerY + playerHeight / 2 - viewportHeight / 2;

            Clamp(roomWidth, roomHeight);
        }

        private void Clamp(double roomWidth, double roomHeight)
        {
            X = Math.Clamp(X, 0, Math.Max(0, roomWidth - viewportWidth));
            Y = Math.Clamp(Y, 0, Math.Max(0, roomHeight - viewportHeight));
        }
    }
}