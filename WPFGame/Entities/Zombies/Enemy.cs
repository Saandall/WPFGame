using System.Windows.Media;
using System.Windows.Shapes;
using WPFGame.Core; // Подключаем папку Core, где лежит Entity

namespace WPFGame.Enemies
{
   // Наследуемся от Entity! Теперь у нас есть HitBox, гравитация и физика.
   public class Enemy : Entity
   {
      // protected set позволяет наследникам (например, Зомби) менять это значение
      public int Health { get; protected set; }

      public Enemy(double startX, double startY, int maxHealth)
      {
         X = startX;
         Y = startY;
         Width = 40;
         Height = 50;
         Health = maxHealth;

         // Рисуем красного болванчика
         VisualShape = new Rectangle
         {
            Width = this.Width,
            Height = this.Height,
            Fill = Brushes.Red
         };
      }

      // Делаем метод virtual, чтобы Зомби и Роботы могли менять логику урона
      public virtual bool TakeDamage(int damage)
      {
         Health -= damage;
         return Health <= 0;
      }
   }
}