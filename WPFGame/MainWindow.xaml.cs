using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading; // Нужно для таймера!
using System.Windows.Shapes;

namespace WPFGame
{
   public partial class MainWindow : Window
   {
      // Переменные для хранения координат игрока
      private double playerX = 100;
      private double playerY = 100;

      // Для движения 
      private bool goLeft = false;
      private bool goRight = false;
      private bool goUp = false;
      private bool goDown = false;
      private bool jumping = false;
      private bool isClimbing = false;

      private bool facingRight = true; // По умолчанию смотрим вправо

      private int speed = 5;
      private double gravity = 0.8; // Сила гравитации 
      private double velocityY = 0.0; // текущая вертикальная скорость
      private int dropCooldown = 0; // Таймер отключения платформ при спрыгивании

      // пистоль
      private Weapon currentWeapon = new Pistol();

      // Список всех летящих прямо сейчас пуль
      private List<Bullet> activeBullets = new List<Bullet>();
      // Список с врагами
      private List<Enemy> activeEnemies = new List<Enemy>();
      // Создаем таймер
      private DispatcherTimer gameTimer = new DispatcherTimer();

      public MainWindow()
      {
         InitializeComponent();

         // Создаем манекен (X=400, Y=300 - чтобы он стоял на полу), 50 здоровья
         Enemy dummy = new Enemy(400, 300, 100);
         activeEnemies.Add(dummy);                // Добавляем в мозг игры
         GameArea.Children.Add(dummy.VisualShape);// Добавляем на экран

         // Настраиваем таймер: как часто он будет "тикать"
         // TimeSpan.FromMilliseconds(16) — это примерно 60 кадров в секунду (1000мс / 60)
         gameTimer.Interval = TimeSpan.FromMilliseconds(16);

         // Говорим таймеру: "При каждом тике вызывай метод GameTick"
         gameTimer.Tick += GameTick;

         // Запускаем таймер!
         gameTimer.Start();
      }

      // Версия где ГГ свободен в перемещениях по лестнице.
      private void GameTick(object sender, EventArgs e)
      {
         Rect playerHitBox = new Rect(playerX, playerY, Player.Width, Player.Height);

         bool onGround = false;
         bool touchingLadder = false;


         // НОВЫЕ ПЕРЕМЕННЫЕ ДЛЯ ПЛАВНОГО ПОЛА
         // Ставим изначально "пол" где-то бесконечно низко
         double highestFloorY = double.MaxValue;
         bool foundFloor = false;
         double feetY = playerY + Player.Height;

         // Уменьшаем таймер каждый кадр
         if (dropCooldown > 0) dropCooldown--;

         // Если нажата комбинация ВНИЗ + ПРОБЕЛ
         if (goDown && jumping)
         {
            dropCooldown = 15; // Даем "неосязаемость" к платформам на 15 кадров
         }

         // Флаг: можно ли нам стоять на пропускаемых поверхностях?
         // (Если таймер равен 0, значит можно. Если больше 0 - мы падаем сквозь них)
         bool canStandOnPlatforms = (dropCooldown == 0);

         // 1. ЦИКЛ КОЛЛИЗИЙ
         foreach (var element in GameArea.Children.OfType<Rectangle>())
         {
            Rect elementHitBox = new Rect(Canvas.GetLeft(element), Canvas.GetTop(element) == double.NaN ? 0 : Canvas.GetTop(element), element.Width, element.Height);

            if (playerHitBox.IntersectsWith(elementHitBox))
            {
               // А) ТВЕРДЫЙ ПОЛ
               if ((string)element.Tag == "Ground")
               {
                  double floorY = Canvas.GetTop(element);
                  if (velocityY >= 0 && feetY >= floorY && playerY < floorY)
                  {
                     if (floorY < highestFloorY) highestFloorY = floorY; // Запоминаем самый высокий пол!
                     foundFloor = true;
                  }
               }
               // Б) ПРОПУСКАЕМАЯ ПЛАТФОРМА (Балкон)
               else if ((string)element.Tag == "Platform")
               {
                  double platformTop = Canvas.GetTop(element);

                  if (canStandOnPlatforms && velocityY >= 0 && feetY >= platformTop && feetY <= platformTop + 15)
                  {
                     if (platformTop < highestFloorY) highestFloorY = platformTop;
                     foundFloor = true;
                  }
               }
               // В) ЛЕСТНИЦА
               else if ((string)element.Tag == "Ladder")
               {
                  touchingLadder = true;
                  
               }
               // Ступенчатая лестница /
               else if ((string)element.Tag == "SlopeUpRight")
               {
                  double slopeLeft = Canvas.GetLeft(element);
                  double slopeWidth = element.Width;
                  double slopeHeight = element.Height;
                  double slopeBottom = Canvas.GetTop(element) + slopeHeight;

                  // Считаем правый край игрока по X
                  double targetX = playerX + Player.Width;
                  double progress = (targetX - slopeLeft) / slopeWidth;

                  progress = Math.Max(0, Math.Min(1, progress)); // Защита от выхода за рамки

                  double currentFloorY = slopeBottom - (progress * slopeHeight);

                  // Проверяем: падаем ли мы, и находятся ли наши ноги рядом с диагональю (допуск 20 пикселей)
                  if (canStandOnPlatforms && velocityY >= 0 && feetY >= currentFloorY - 15 && feetY <= currentFloorY + 20)
                  {
                     if (currentFloorY < highestFloorY) highestFloorY = currentFloorY;
                     foundFloor = true;
                  }
               }
               // Ступенчатая лестница \
               else if ((string)element.Tag == "SlopeUpLeft")
               {
                  double slopeLeft = Canvas.GetLeft(element);
                  double slopeTop = Canvas.GetTop(element);
                  double slopeWidth = element.Width;
                  double slopeHeight = element.Height;
                  double slopeBottom = slopeTop + slopeHeight;

                  double targetX = playerX;
                  double progress = (targetX - slopeLeft) / slopeWidth;
                  progress = Math.Max(0, Math.Min(1, progress));

                  // Математика ИНАЯ: пол начинается вверху (slopeTop) и спускается к низу (slopeBottom)
                  double currentFloorY = slopeTop + (progress * slopeHeight);

                  if (canStandOnPlatforms && velocityY >= 0 && feetY >= currentFloorY - 15 && feetY <= currentFloorY + 20)
                  {
                     if (currentFloorY < highestFloorY) highestFloorY = currentFloorY;
                     foundFloor = true;
                  }
               }
            }
         }

         if (foundFloor)
         {
            playerY = highestFloorY - Player.Height;
            velocityY = 0;
            onGround = true;
         }

         // 2. ЛОГИКА ЛЕСТНИЦЫ

         // Если ушли с лестницы вбок или долезли до верха -> отцепляемся
         if (!touchingLadder)
         {
            isClimbing = false;
         }

         // ВХОД НА ЛЕСТНИЦУ: Касаемся любым пикселем, не лезем, и нажали Вверх/Вниз
         if (touchingLadder && !isClimbing && (goUp || goDown))
         {
            isClimbing = true;
            velocityY = 0; // Гасим скорость падения, но НЕ телепортируем по X
         }

         // 3. ФИЗИКА И ДВИЖЕНИЕ

         // Движение Влево-Вправо теперь работает ВСЕГДА (и в воздухе, и на полу, и на лестнице)
         if (goLeft)
         {
            playerX -= speed;
            facingRight = false;
         }
            if (goRight) 
         {
            playerX += speed;
            facingRight = true;
         }
         if (isClimbing)
         {
            // Режим лазания: Гравитация ОТКЛЮЧЕНА, двигаемся вверх-вниз
            velocityY = 0;
            if (goUp) playerY -= speed;
            if (goDown) playerY += speed;
         }
         else
         {
            // Обычный режим: Гравитация ВКЛЮЧЕНА
            velocityY += gravity;
            playerY += velocityY;
         }

         // Прыжок (С лестницы прыгать нельзя, нужно сначала сойти вбок)
         if (jumping && onGround && !isClimbing && !goDown)
         {
            velocityY = -15;
         }

         // Ограничение экрана
         if (playerX < 0) playerX = 0;
         if (playerX > 850) playerX = 850;

         // Полёт пулек
         List<Bullet> bulletsToRemove = new List<Bullet>(); // Список пуль "на удаление"
         List<Enemy> enemiesToRemove = new List<Enemy>(); // Список убитых врагов
         foreach (var bullet in activeBullets)
         {
            // Двигаем пулю по математике
            bullet.X += bullet.Speed;

            // Двигаем пулю визуально на Canvas
            Canvas.SetLeft(bullet.VisualShape, bullet.X);

            // Хитбокс пули
            Rect bulletHitBox = new Rect(bullet.X, bullet.Y, bullet.VisualShape.Width, bullet.VisualShape.Height);
            bool hitSomething = false;

            // Проверяем столкновение пули с каждым врагом
            foreach (var enemy in activeEnemies)
            {
               Rect enemyHitBox = new Rect(enemy.X, enemy.Y, enemy.VisualShape.Width, enemy.VisualShape.Height);

               if (bulletHitBox.IntersectsWith(enemyHitBox))
               {
                  hitSomething = true; // Пуля во что-то попала!

                  // Враг получает урон
                  bool isDead = enemy.TakeDamage(bullet.Damage);

                  // Если враг умер, помечаем его на удаление
                  if (isDead && !enemiesToRemove.Contains(enemy))
                  {
                     enemiesToRemove.Add(enemy);
                  }
                  break; // Пуля исчезает при первом же попадании, дальше врагов не проверяем
               }
            }

            // Если пуля улетела за края экрана (допустим, 0 и 800) -> помечаем на удаление
            if (hitSomething || bullet.X < 0 || bullet.X > 900)
            {
               bulletsToRemove.Add(bullet);
            }
         }
         // Очищаем мусор (удаляем улетевшие пули с экрана и из памяти)
         foreach (var bullet in bulletsToRemove)
         {
            GameArea.Children.Remove(bullet.VisualShape); // Удаляем картинку
            activeBullets.Remove(bullet);                 // Удаляем из математики
         }

         foreach (var enemy in enemiesToRemove)
         {
            GameArea.Children.Remove(enemy.VisualShape);
            activeEnemies.Remove(enemy);
         }

         // 4. ВИЗУАЛ
         Canvas.SetLeft(Player, playerX);
         Canvas.SetTop(Player, playerY);
      }

      // Для управления
      // Нажатая кнопка
      private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
      {
         if (e.Key == System.Windows.Input.Key.Left) goLeft = true;
         if (e.Key == System.Windows.Input.Key.Right) goRight = true;
         if (e.Key == System.Windows.Input.Key.Space || e.Key == System.Windows.Input.Key.Up) jumping = true;
         if (e.Key == System.Windows.Input.Key.Up) goUp = true;
         if (e.Key == System.Windows.Input.Key.Down) goDown = true;
      }

      // Отжатая кнопка
      private void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
      {
         if (e.Key == System.Windows.Input.Key.Left) goLeft = false;
         if (e.Key == System.Windows.Input.Key.Right) goRight = false;
         if (e.Key == System.Windows.Input.Key.Space || e.Key == System.Windows.Input.Key.Up) jumping = false;
         if (e.Key == System.Windows.Input.Key.Up) goUp = false;
         if (e.Key == System.Windows.Input.Key.Down) goDown = false;
         if (e.Key == System.Windows.Input.Key.LeftCtrl) currentWeapon.Attack(GameArea, playerX, playerY, facingRight, activeBullets);
      }
   }

   public class Bullet
   {
      public System.Windows.Shapes.Rectangle VisualShape { get; private set; }
      public double X { get; set; }
      public double Y { get; set; }
      public double Speed { get; private set; }
      public int Damage { get; private set; }

      public Bullet (double startX,  double startY, double speed, bool movingRight, int damage)
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

         // Начальные координаты пульки
         Canvas.SetLeft(VisualShape, X);
         Canvas.SetTop(VisualShape, Y);
      }
   }

   // Базовый класс любого оружия
   public abstract class Weapon
   {
      public string Name { get; set; }
      public int Damage { get; set; }

      public abstract void Attack(Canvas GameArea, double playerX, double playerY, bool facingRight, List<Bullet> activeBullets);
   }

   public class Pistol : Weapon
   { 
      public Pistol()
      {
         Name = "Colt Python";
         Damage = 25;
      }

      public override void Attack(Canvas GameArea, double playerX, double playerY, bool facingRight, List<Bullet> activeBullets)
      {
         double spawnX = facingRight ? playerX + 50 : playerX - 10;
         double spawnY = playerY + 20;

         // Пулька
         Bullet newBullet = new Bullet(spawnX, spawnY, 15, facingRight, Damage);

         // Добавляем пульку физически
         activeBullets.Add(newBullet);

         // Добавляем пульку визуально
         GameArea.Children.Add(newBullet.VisualShape);
      }
   }

   public class Enemy
   {
      public System.Windows.Shapes.Rectangle VisualShape { get; private set; }
      public double X { get; set; }
      public double Y { get; set; }
      public int Health { get; private set; }
      public Enemy(double startX, double startY, int maxHealth)
      {
         X = startX;
         Y = startY;
         Health = maxHealth;

         // Рисуем красного болванчика
         VisualShape = new System.Windows.Shapes.Rectangle
         {
            Width = 40,
            Height = 50,
            Fill = System.Windows.Media.Brushes.Red
         };

         Canvas.SetLeft(VisualShape, X);
         Canvas.SetTop(VisualShape, Y);
      }

      // Метод получения урона. 
      // Возвращает true, если враг умер (чтобы GameTick знал, что его пора удалить)
      public bool TakeDamage(int damage)
      {
         Health -= damage;
         return Health <= 0;
      }
   }

}