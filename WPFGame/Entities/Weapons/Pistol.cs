using System.Windows;
using System.Windows.Controls;
using WPFGame.Core;
using WPFGame.Enemies;

namespace WPFGame.Weapons
{
   public class Pistol : Weapon
   {
      public Pistol()
      {
         Name = "Colt Python";
         Damage = 25;
         MaxAmmo = 6;
         Ammo = 6;
         ReserveAmmo = 1024;
         reloadTimeFrames = 90; // количество кадров длительности перезарядки (90 кадров = 1.5 секунды)

         IsAutomatic = false;
         fireRateFrames = 15; // Количество кадров задержки между выстрелами 
      }

      // Поиск границы tile, чтобы прервать вектор. Поиск - бинарный
      private Point FindCollisionPoint(Point freePoint, Point hitPoint, Func<Point, bool> isCollision)
      {
         Point low = freePoint;
         Point high = hitPoint;

         // 6 итераций дают достаточно высокую точность
         // на отрезке длиной 10 пикселей.
         for (int i = 0; i < 6; i++)
         {
            Point middle = new Point(
                (low.X + high.X) / 2,
                (low.Y + high.Y) / 2);

            if (isCollision(middle))
            {
               high = middle;
            }
            else
            {
               low = middle;
            }
         }

         return high;
      }

      public override void Attack(Canvas GameArea, double playerX, double playerY, List<WPFGame.Enemies.Enemy> enemies,
                            System.Windows.Controls.UIElementCollection mapElements,
                            List<System.Windows.Shapes.Line> tracers)
      {

         if (IsReloading || Ammo <= 0 || fireCooldownTimer > 0 || (!IsAutomatic && !triggerReady)) return;

         Ammo -= 1;

         fireCooldownTimer = fireRateFrames; // Та самая задержка
         triggerReady = false;               // Не позволяет "зажать" с пистолета и стрелять как автомат. Только одиночные выстрелы

         // Автоматическая перезарядка
         if (Ammo == 0 && ReserveAmmo > 0)
         {
            Reload();
         }

         // --- Логика стрельбы ---
         // В дальнейшем (при добавлении дробовика и автомата) общая логика перейдёт в отдельный файл
         
         // Точка "отсчёта" для вектора
         double startX = playerX;
         double startY = playerY;

         // разница координат курсора и точки "отсчёта"
         double dx = Inputmanager.MouseX - startX;
         double dy = Inputmanager.MouseY - startY;

         double distance = Math.Sqrt(dx * dx + dy * dy);

         if (distance < 0.001)
            return;

         double dirX = dx / distance;
         double dirY = dy / distance;

         // RAYCASTING 
         double currentX = startX;
         double currentY = startY;
         double maxDistance = 1000; // Наибольшая длина вектора выстрела
         double rayStep = 10; // Шаг, через который проверяется, столкнулся ли вектор с Тайлом
         bool hitSomething = false;

         for (double traveled = 0; traveled < maxDistance; traveled += rayStep)
         {
            Point previousPoint = new Point(currentX, currentY);
            currentX += dirX * rayStep;
            currentY += dirY * rayStep;

            // Точка проверки
            Point checkPoint = new Point(currentX, currentY);
            Enemy hitEnemy = null;
            
            // Логика попадания во врага
            foreach (var enemy in enemies)
            {
               if (enemy.HitBox.Contains(checkPoint))
               {
                  Point collisionPoint = FindCollisionPoint(previousPoint, checkPoint, point => enemy.HitBox.Contains(point));

                  currentX = collisionPoint.X;
                  currentY = collisionPoint.Y;

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

            // Логика попадания в Ground и Platform 
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
                        Point collisionPoint = FindCollisionPoint(previousPoint, checkPoint, point => wallHitBox.Contains(point));

                        currentX = collisionPoint.X;
                        currentY = collisionPoint.Y;

                        hitSomething = true;
                        break;
                     }
                  }
               }
            }

            if (hitSomething) break;
         }

         // Отрисовка вектора
         System.Windows.Shapes.Line tracer = new System.Windows.Shapes.Line
         {
            X1 = startX,
            Y1 = startY,
            X2 = currentX,
            Y2 = currentY,
            Stroke = System.Windows.Media.Brushes.White, // Покрашен в белый как в Final Station
            StrokeThickness = 1, // Толщина вектора
            Opacity = 1.0 
         };
         tracer.Tag = 5; // Количество кадров, на которых будет виден вектор

         GameArea.Children.Add(tracer);
         tracers.Add(tracer);
      }


   }
}