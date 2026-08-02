using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading; // Нужно для таймера!
using System.Windows.Shapes;
using WPFGame.Level;

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


      private int speed = 5;
      private double gravity = 0.8; // Сила гравитации 
      private double velocityY = 0.0; // текущая вертикальная скорость
      
      
      // Создаем таймер
      private DispatcherTimer gameTimer = new DispatcherTimer();

      // Хранит текущую комнату и умеет переключать её на соседнюю
      private RoomManager roomManager;

      public MainWindow()
      {
         InitializeComponent();

         // RoomManager сам спавнит стартовую комнату и дальше сам следит,
         // что сейчас лежит на Canvas — нам об этом заботиться больше не нужно.
         roomManager = new RoomManager(GameArea, TestLevel.StartRoom);

         playerX = roomManager.CurrentRoom.PlayerStartX;
         playerY = roomManager.CurrentRoom.PlayerStartY;
         Canvas.SetLeft(Player, playerX);
         Canvas.SetTop(Player, playerY);

         // Настраиваем таймер: как часто он будет "тикать"
         // TimeSpan.FromMilliseconds(16) — это примерно 60 кадров в секунду (1000мс / 60)
         gameTimer.Interval = TimeSpan.FromMilliseconds(16);

         // Говорим таймеру: "При каждом тике вызывай метод GameTick"
         gameTimer.Tick += GameTick;

         // Запускаем таймер!
         gameTimer.Start();
      }

      // Этот метод вызывается АВТОМАТИЧЕСКИ 60 раз в секунду

      //// Версия с тем, где ГГ цепляется к центру лестницы
      //private void GameTick(object sender, EventArgs e)
      //{
      //   // 1. Создаем Хитбокс игрока (математическую рамку вокруг его текущих координат)
      //   Rect playerHitBox = new Rect(playerX, playerY, Player.Width, Player.Height);

      //   bool onGround = false;       // Стоим ли мы на твердом полу?
      //   bool touchingLadder = false; // Касаемся ли мы лестницы?

      //   double activeLadderCenter = 0.0; // Нужен для координаты центра лестницы

      //   // 2. ЦИКЛ КОЛЛИЗИЙ: Проверяем все прямоугольники на нашем Canvas
      //   foreach (var element in GameArea.Children.OfType<Rectangle>())
      //   {
      //      // Создаем Хитбокс для текущего элемента сцены (пола или лестницы)
      //      Rect elementHitBox = new Rect(Canvas.GetLeft(element), Canvas.GetTop(element) == double.NaN ? 0 : Canvas.GetTop(element), element.Width, element.Height);

      //      // Если Игрок ПЕРЕСЕКАЕТСЯ с этим элементом
      //      if (playerHitBox.IntersectsWith(elementHitBox))
      //      {
      //         // Проверяем бирки (Tag)
      //         if ((string)element.Tag == "Ground")
      //         {
      //            // Останавливаемся всегда, если ноги коснулись пола (даже если лезем по лестнице!)
      //            if (playerY + Player.Height >= Canvas.GetTop(element) && playerY < Canvas.GetTop(element))
      //            {
      //               playerY = Canvas.GetTop(element) - Player.Height; // Ставим ровно на пол
      //               velocityY = 0;
      //               onGround = true;
      //            }
      //         }
      //         else if ((string)element.Tag == "Platform")
      //         {
      //            // Цепляемся за нее только если ПАДАЕМ или СТОИМ, 
      //            // и ГЛАВНОЕ — НЕ жмем Вниз или Вверх (не находимся в процессе лазания)
      //            double feetY = playerY + Player.Height;
      //            double platformTop = Canvas.GetTop(element);

      //            if (velocityY >= 0 && !goDown && !goUp && feetY >= platformTop && feetY <= platformTop + 15)
      //            {
      //               playerY = platformTop - Player.Height;
      //               velocityY = 0;
      //               onGround = true;
      //            }
      //         }
      //         else if ((string)element.Tag == "Ladder")
      //         {
      //            touchingLadder = true;
      //            activeLadderCenter = Canvas.GetLeft(element) + (element.Width / 2);
      //         }
      //      }
      //   }

      //   // 3. ЛОГИКА ЛЕСТНИЦЫ (State Machine)
      //   if (!touchingLadder)
      //   {
      //      // Если ушли с лестницы -> падаем
      //      isClimbing = false;
      //   }

      //   if (touchingLadder && !isClimbing && (goUp || goDown))
      //   {
      //      // Если мы у лестницы и нажали Вверх/Вниз -> начинаем лезть!
      //      isClimbing = true;
      //      playerX = activeLadderCenter - (Player.Width / 2);
      //      velocityY = 0;
      //   }

      //   // В) ВЫХОД С ЛЕСТНИЦЫ ВНИЗУ: Если мы висим на лестнице, стоим на полу и жмем вбок
      //   if (isClimbing && onGround && (goLeft || goRight))
      //   {
      //      isClimbing = false; // Отпускаем лестницу!
      //   }



      //   // 4. ФИЗИКА И ДВИЖЕНИЕ
      //   if (isClimbing)
      //   {
      //      // Режим лазания: Гравитация ОТКЛЮЧЕНА
      //      velocityY = 0;
      //      if (goUp) playerY -= speed;
      //      if (goDown) playerY += speed;
      //   }
      //   else
      //   {
      //      // Обычный режим: Гравитация ВКЛЮЧЕНА
      //      velocityY += gravity;
      //      playerY += velocityY;

      //      // Только когда мы НЕ на лестнице, мы можем бегать влево-вправо
      //      if (goLeft) playerX -= speed;
      //      if (goRight) playerX += speed;
      //   }

      //   // Прыжок (Только если мы на земле и НЕ лезем по лестнице)
      //   if (jumping && onGround && !isClimbing)
      //   {
      //      velocityY = -15;
      //   }

      //   // Ограничение экрана
      //   if (playerX < 0) playerX = 0;
      //   if (playerX > 750) playerX = 750;

      //   // 5. ВИЗУАЛ
      //   Canvas.SetLeft(Player, playerX);
      //   Canvas.SetTop(Player, playerY);
      //}

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

                  if (velocityY >= 0 && !goDown && feetY >= platformTop && feetY <= platformTop + 15)
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
                  if (velocityY >= 0 && feetY >= currentFloorY && feetY <= currentFloorY + 20)
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

                  if (velocityY >= 0 && feetY >= currentFloorY && feetY <= currentFloorY + 20)
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
         if (goLeft) playerX -= speed;
         if (goRight) playerX += speed;

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
         if (jumping && onGround && !isClimbing)
         {
            velocityY = -15;
         }

         // Ограничение экрана — теперь по ширине ТЕКУЩЕЙ комнаты, а не фиксированное число.
         // Иначе игрок физически не смог бы дойти до двери у правого края большой комнаты.
         double maxX = roomManager.CurrentRoom.Width - Player.Width;
         if (playerX < 0) playerX = 0;
         if (playerX > maxX) playerX = maxX;

         // Проверка перехода в другую комнату: коснулись ли края комнаты там, где есть дверь.
         // Если да — RoomManager сам подменяет комнату на Canvas и говорит, куда поставить игрока.
         Rect currentHitBox = new Rect(playerX, playerY, Player.Width, Player.Height);
         var transition = roomManager.TryTransition(currentHitBox);
         if (transition is not null)
         {
            (playerX, playerY) = transition.Value;
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
         if (e.Key == System.Windows.Input.Key.Space) jumping = true;
         if (e.Key == System.Windows.Input.Key.Up) goUp = true;
         if (e.Key == System.Windows.Input.Key.Down) goDown = true;
      }

      // Отжатая кнопка
      private void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
      {
         if (e.Key == System.Windows.Input.Key.Left) goLeft = false;
         if (e.Key == System.Windows.Input.Key.Right) goRight = false;
         if (e.Key == System.Windows.Input.Key.Space) jumping = false;
         if (e.Key == System.Windows.Input.Key.Up) goUp = false;
         if (e.Key == System.Windows.Input.Key.Down) goDown = false;
      }
   }
}