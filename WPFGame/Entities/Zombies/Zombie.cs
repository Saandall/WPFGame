using System.Windows.Media;

namespace WPFGame.Enemies
{
   public class Zombie : Enemy
   {
      // ѕередаем X, Y и ’ѕ в базовый конструктор Enemy
      public Zombie(double startX, double startY, int maxHealth) : base(startX, startY, maxHealth)
      {
         // ћен€ем цвет, чтобы отличать «омби от манекена
         VisualShape.Fill = Brushes.DarkGreen;
      }

      // ѕереопредел€ем урон («омби получает в 2 раза меньше урона)
      public override bool TakeDamage(int damage)
      {
         Health -= (damage / 2);
         return Health <= 0;
      }
   }
}

// Ќ≈ ƒќЅј¬Ћя“№!