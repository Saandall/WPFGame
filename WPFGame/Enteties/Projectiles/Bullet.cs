
using System.Windows;
using System.Windows.Controls;

namespace WPFGame.Projectiles
{
   public class Bullet
   {
      public System.Windows.Shapes.Rectangle VisualShape { get; private set; }
      public double X { get; set; }
      public double Y { get; set; }
      public double Speed { get; private set; }
      public int Damage { get; private set; }

      public Bullet(double startX, double startY, double speed, bool movingRight, int damage)
      {
         X = startX;
         Y = startY;
         Speed = movingRight ? speed : -speed;
         Damage = damage;

         // Графика пульки
         VisualShape = new System.Windows.Shapes.Rectangle
         {
            Width = 10,
            Height = 4,
            Fill = System.Windows.Media.Brushes.Yellow
         };
      }

      // Пуля сама себя двигает!
      public void Update()
      {
         X += Speed;
         Canvas.SetLeft(VisualShape, X);
         Canvas.SetTop(VisualShape, Y);
      }

      // Пуля сама знает, когда ей пора исчезнуть
      public bool IsOutOfBounds()
      {
         return X < 0 || X > 800; // 800 - ширина экрана (пока захардкодим)
      }

      // Хитбокс пули для проверки попаданий
      public Rect HitBox => new Rect(X, Y, VisualShape.Width, VisualShape.Height);

      // Начальные координаты пульки
      //Canvas.SetLeft(VisualShape, X);
      //Canvas.SetTop(VisualShape, Y);
   }
}