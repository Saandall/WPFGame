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


      private int speed = 5;
      private double gravity = 0.8; // Сила гравитации 
      private double velocityY = 0.0; // текущая вертикальная скорость
      
      
      // Создаем таймер
      private DispatcherTimer gameTimer = new DispatcherTimer();

      public MainWindow()
      {
         InitializeComponent();

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

         // 1. ЦИКЛ КОЛЛИЗИЙ
         foreach (var element in GameArea.Children.OfType<Rectangle>())
         {
            Rect elementHitBox = new Rect(Canvas.GetLeft(element), Canvas.GetTop(element) == double.NaN ? 0 : Canvas.GetTop(element), element.Width, element.Height);

            if (playerHitBox.IntersectsWith(elementHitBox))
            {
               // А) ТВЕРДЫЙ ПОЛ
               if ((string)element.Tag == "Ground")
               {
                  if (playerY + Player.Height >= Canvas.GetTop(element) && playerY < Canvas.GetTop(element))
                  {
                     playerY = Canvas.GetTop(element) - Player.Height;
                     velocityY = 0;
                     onGround = true;
                  }
               }
               // Б) ПРОПУСКАЕМАЯ ПЛАТФОРМА (Балкон)
               else if ((string)element.Tag == "Platform")
               {
                  double feetY = playerY + Player.Height;
                  double platformTop = Canvas.GetTop(element);

                  if (velocityY >= 0 && !goDown && feetY >= platformTop && feetY <= platformTop + 15)
                  {
                     playerY = platformTop - Player.Height;
                     velocityY = 0;
                     onGround = true;
                  }
               }
               // В) ЛЕСТНИЦА
               else if ((string)element.Tag == "Ladder")
               {
                  touchingLadder = true;
                  // Мы удалили расчет центра лестницы
               }
            }
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

         // Ограничение экрана
         if (playerX < 0) playerX = 0;
         if (playerX > 750) playerX = 750;

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