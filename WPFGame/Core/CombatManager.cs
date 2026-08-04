using System.Collections.Generic;
using System.Windows.Controls;
using WPFGame.Enemies;
using WPFGame.Projectiles;

namespace WPFGame.Core
{
   public static class CombatManager
   {
      // Метод обрабатывает все летящие пули и проверяет столкновения с врагами
      public static void UpdateBulletsAndHits(List<Bullet> activeBullets, List<Enemy> activeEnemies, Canvas gameArea, double roomWidth)
      {
         List<Bullet> bulletsToRemove = new List<Bullet>();
         List<Enemy> enemiesToRemove = new List<Enemy>();

         foreach (var bullet in activeBullets)
         {
            bullet.Update(); // Двигаем пулю

            bool hitSomething = false;

            // Проверяем столкновение с каждым живым врагом
            foreach (var enemy in activeEnemies)
            {
               if (bullet.HitBox.IntersectsWith(enemy.HitBox))
               {
                  hitSomething = true;

                  // Враг получает урон (если метод вернул true, значит враг убит)
                  if (enemy.TakeDamage(bullet.Damage) && !enemiesToRemove.Contains(enemy))
                  {
                     enemiesToRemove.Add(enemy);
                  }
                  break; // Пуля исчезает об первого же врага
               }
            }

            if (hitSomething || bullet.IsOutOfBounds(roomWidth))
            {
               bulletsToRemove.Add(bullet);
            }
         }

         // Очищаем мусор (удаляем из списков и убираем картинки с экрана)
         foreach (var b in bulletsToRemove)
         {
            gameArea.Children.Remove(b.VisualShape);
            activeBullets.Remove(b);
         }

         foreach (var e in enemiesToRemove)
         {
            gameArea.Children.Remove(e.VisualShape);
            activeEnemies.Remove(e);
         }
      }
   }
}