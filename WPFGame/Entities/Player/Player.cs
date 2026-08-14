using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WPFGame.Core;

namespace WPFGame.PlayerLogic
{
    public class Player : Entity
    {
        public bool IsClimbing { get; private set; }
        public bool FacingRight { get; private set; } = true;

        // Запрещает автоматический прыжок после выхода с лестницы
        private bool preventAutoJump;

        // На несколько кадров отключает столкновение с платформами
        private int dropCooldown;

        public Player(
            double startX,
            double startY)
        {
            X = startX;
            Y = startY;

            Width = 20;
            Height = 50;

            VisualShape =
                new Rectangle
                {
                    Width =
                        Width,

                    Height =
                        Height,

                    Fill =
                        Brushes.LimeGreen
                };
        }

        // Обновляет управление и физику, не ограничивая мировые координаты
        public void Update(
            UIElementCollection mapElements)
        {
            // Сохраняются координаты до обычного движения игрока.
            // Entity использует их только для определения стороны столкновения.
            double previousX =
                X;

            double previousY =
                Y;

            bool goLeft =
                Inputmanager.GoLeft;

            bool goRight =
                Inputmanager.GoRight;

            bool goUp =
                Inputmanager.GoUp;

            bool goDown =
                Inputmanager.GoDown;

            bool jumping =
                Inputmanager.Jumping;

            if (!jumping)
            {
                preventAutoJump =
                    false;
            }

            if (goLeft)
            {
                FacingRight =
                    false;
            }

            if (goRight)
            {
                FacingRight =
                    true;
            }

            if (dropCooldown > 0)
            {
                dropCooldown--;
            }

            if (goDown &&
                jumping &&
                OnGround)
            {
                dropCooldown =
                    10;
            }

            bool canStandOnPlatforms =
                dropCooldown == 0 &&
                !IsClimbing;

            double currentGravity =
                0.8;

            if (IsClimbing)
            {
                currentGravity =
                    0;

                VelocityY =
                    0;

                if (goUp)
                {
                    Y -=
                        5;
                }

                if (goDown)
                {
                    Y +=
                        5;
                }
            }

            // Горизонтальное движение остаётся таким же,
            // как в стабильной версии до добавления стен.
            if (goLeft)
            {
                X -=
                    15;
            }

            if (goRight)
            {
                X +=
                    15;
            }

            base.UpdatePhysics(
                mapElements,
                currentGravity,
                canStandOnPlatforms,
                previousX,
                previousY);

            if (!TouchingLadder)
            {
                if (IsClimbing)
                {
                    preventAutoJump =
                        true;
                }

                IsClimbing =
                    false;
            }

            if (TouchingLadder &&
                !IsClimbing)
            {
                double feetY =
                    Y +
                    Height;

                if (feetY <=
                    ActiveLadderTop + 10)
                {
                    if (goDown)
                    {
                        IsClimbing =
                            true;

                        VelocityY =
                            0;
                    }
                }
                else if (goUp ||
                         goDown)
                {
                    IsClimbing =
                        true;

                    VelocityY =
                        0;
                }
            }

            if (IsClimbing &&
                OnGround &&
                (goLeft ||
                 goRight) &&
                !goUp &&
                !goDown)
            {
                IsClimbing =
                    false;
            }

            if (jumping &&
                OnGround &&
                !IsClimbing &&
                !goDown &&
                !preventAutoJump)
            {
                VelocityY =
                    -15;
            }
        }
    }
}
