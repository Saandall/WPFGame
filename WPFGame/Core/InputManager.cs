using System.Windows.Input;

namespace WPFGame.Core
{
   public static class Inputmanager
   {
      // Глобальные флаги состояния кнопок
      public static bool GoLeft { get; private set; }
      public static bool GoRight { get; private set; }
      public static bool GoUp { get; private set; }
      public static bool GoDown { get; private set; }
      public static bool Jumping { get; private set; }
      public static bool Shooting { get; private set; }
      public static bool Reloading { get; private set; }

      // Метод, который переводит нажатия кнопок WPF в наши флаги
      public static void UpdateKeyState(Key key, bool isPressed)
      {
         if (key == Key.Left || key == Key.A) GoLeft = isPressed;
         if (key == Key.Right || key == Key.D) GoRight = isPressed;
         if (key == Key.Up || key == Key.W) GoUp = isPressed;
         if (key == Key.Down || key == Key.S) GoDown = isPressed;
         if (key == Key.Space || key == Key.Up || key == Key.W) Jumping = isPressed;
         if (key == Key.Z) Shooting = isPressed;
         if (key == Key.R) Reloading = isPressed;
      }
   }
}