using System.Windows;
using System.Windows.Controls;
using WPFGame.Core;
using WPFGame.Enemies;
using WPFGame.Projectiles;

namespace WPFGame.Weapons
{
   public class Pistol : Weapon
   {
      public Pistol()
      {
         Name = "Colt Python";
         Damage = 25;
         MaxAmmo = 6;
         Ammo = 106;
         ReserveAmmo = 1024;
         reloadTimeFrames = 90; // Пусть пистолет перезаряжается 1.5 секунды (90 кадров)

         IsAutomatic = false;
         fireRateFrames = 15; // четверть секунды
      }

      public override void Attack(Canvas GameArea, double playerX, double playerY, List<WPFGame.Enemies.Enemy> enemies,
                            System.Windows.Controls.UIElementCollection mapElements,
                            List<System.Windows.Shapes.Line> tracers)
      {

         if (IsReloading || Ammo <= 0 || fireCooldownTimer > 0 || (!IsAutomatic && !triggerReady)) return;

         Ammo -= 1;

         // --- ПОСЛЕ ВЫСТРЕЛА ---
         fireCooldownTimer = fireRateFrames; // Запускаем задержку до следующего выстрела
         triggerReady = false;               // Блокируем курок (пока игрок не отпустит кнопку Z)

         // Автоматическая перезарядка
         if (Ammo == 0 && ReserveAmmo > 0)
         {
            Reload();
         }

         // 1. НАЧАЛЬНАЯ ТОЧКА (Дуло)
         // Изменить при надобности при добавлении графики
         double startX = playerX; // Примерно центр игрока
         double startY = playerY;

         // 2. ВЕКТОР ДО МЫШИ
         double dx = Inputmanager.MouseX - startX;
         double dy = Inputmanager.MouseY - startY;

         // Считаем длину вектора
         double distance = Math.Sqrt(dx * dx + dy * dy);

         if (distance < 0.001)
            return;

         // Нормализация вектора
         double dirX = dx / distance;
         double dirY = dy / distance;

         // 3. RAYCASTING (Пускаем луч)
         double currentX = startX;
         double currentY = startY;
         double maxDistance = 1000; // Пуля не полетит дальше 1000 пикселей
         double rayStep = 10; // Шагаем по 10 пикселей за раз
         bool hitSomething = false;

         for (double traveled = 0; traveled < maxDistance; traveled += rayStep)
         {
            currentX += dirX * rayStep;
            currentY += dirY * rayStep;

            // Создаем микро-точку для проверки
            Point checkPoint = new Point(currentX, currentY);
            Enemy hitEnemy = null;
            // А) Проверяем врагов
            foreach (var enemy in enemies)
            {
               if (enemy.HitBox.Contains(checkPoint))
               {
                  if (enemy.TakeDamage(Damage))
                     hitEnemy = enemy;

                  hitSomething = true;
                  break;
               }
            }

            if (hitEnemy != null)
            {
               GameArea.Children.Remove(hitEnemy.VisualShape);
               enemies.Remove(hitEnemy);
            }

            // Б) Проверяем стены (Ground или Platform)
            if (!hitSomething)
            {
               foreach (var element in mapElements.OfType<System.Windows.Shapes.Rectangle>())
               {
                  string tag = (string)element.Tag;
                  if (tag == "Ground" || tag == "Platform")
                  {
                     Rect wallHitBox = new Rect(Canvas.GetLeft(element), Canvas.GetTop(element), element.Width, element.Height);
                     if (wallHitBox.Contains(checkPoint))
                     {
                        hitSomething = true;
                        break;
                     }
                  }
               }
            }

            // Если во что-то врезались - ЛУЧ ОСТАНАВЛИВАЕТСЯ ЗДЕСЬ!
            if (hitSomething) break;
         }

         // 4. ВИЗУАЛ (Рисуем вспышку от дула до точки попадания/конца полета)
         System.Windows.Shapes.Line tracer = new System.Windows.Shapes.Line
         {
            X1 = startX,
            Y1 = startY,
            X2 = currentX,
            Y2 = currentY,
            Stroke = System.Windows.Media.Brushes.White, // Белый цвет как в Final Station
            StrokeThickness = 2,
            Opacity = 1.0 // Полностью непрозрачный
         };
         tracer.Tag = 5; // количество кадров, которое будет показываться вектор

         GameArea.Children.Add(tracer);
         tracers.Add(tracer); // Добавляем в список, чтобы потом плавно растворить
      }
   }
}
