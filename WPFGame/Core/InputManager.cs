using System.Windows.Input;

namespace WPFGame.Core
{
    public static class Inputmanager
    {
        // Глобальные флаги состояния управляющих кнопок
        public static bool GoLeft { get; private set; }
        public static bool GoRight { get; private set; }
        public static bool GoUp { get; private set; }
        public static bool GoDown { get; private set; }
        public static bool Jumping { get; private set; }
        public static bool Shooting { get; private set; }
        public static bool Reloading { get; private set; }
        public static bool Interacting { get; private set; }

        public static double MouseX { get; set; }
        public static double MouseY { get; set; }

      // Переводит нажатия WPF-клавиш в состояние управления
      public static void UpdateKeyState(
            Key key,
            bool isPressed)
        {
            if (key == Key.Left ||
                key == Key.A)
            {
                GoLeft =
                    isPressed;
            }

            if (key == Key.Right ||
                key == Key.D)
            {
                GoRight =
                    isPressed;
            }

            if (key == Key.Up ||
                key == Key.W)
            {
                GoUp =
                    isPressed;
            }

            if (key == Key.Down ||
                key == Key.S)
            {
                GoDown =
                    isPressed;
            }

            if (key == Key.Space ||
                key == Key.Up ||
                key == Key.W)
            {
                Jumping =
                    isPressed;
            }

            if (key == Key.Z)
            {
                Shooting =
                    isPressed;
            }

            if (key == Key.R)
            {
                Reloading =
                    isPressed;
            }

            if (key == Key.E)
            {
                Interacting =
                    isPressed;
            }
        }
    }
}