using System.Windows.Media;

namespace WPFGame.Enemies
{
   public class Zombie : Enemy
   {
      // Передаем X, Y и ХП в базовый конструктор Enemy
      public Zombie(double startX, double startY, int maxHealth) : base(startX, startY, maxHealth)
      {
         // Меняем цвет, чтобы отличать Зомби от манекена
         VisualShape.Fill = Brushes.DarkGreen;
      }

      // Переопределяем урон (Зомби получает в 2 раза меньше урона)
      public override bool TakeDamage(int damage)
      {
         Health -= (damage / 2);
         return Health <= 0;
      }
   }
}

// НЕ ДОБАВЛЯТЬ!