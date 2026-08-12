//using System;
//using System.Collections.Generic;
//using System.Windows;
//using System.Windows.Input;
//using System.Windows.Threading;
//// Подключаем наши папки:
//using WPFGame.Core;
//using WPFGame.PlayerLogic;
//using WPFGame.Weapons;
//using WPFGame.Enemies;
//using WPFGame.Projectiles;
//using System.Windows.Shapes;
//using WPFGame.Level;
//using System.Windows.Controls;

//namespace WPFGame
//{
//   public partial class MainWindow : Window
//   {
//      private DispatcherTimer gameTimer = new DispatcherTimer();

//      // Списки и объекты, которые живут на уровне
//      private Player myHero;
//      private Weapon currentWeapon;
//      private List<Bullet> activeBullets = new List<Bullet>();
//      private List<Enemy> activeEnemies = new List<Enemy>();

//      // Переменные для хранения координат игрока
//      private double playerX = 100;
//      private double playerY = 100;

//      // Хранит текущую комнату и умеет переключать её на соседнюю
//      private RoomManager roomManager;

//      // Какая часть комнаты сейчас видна
//      private CameraController camera = new CameraController(viewportWidth: 960, viewportHeight: 540, deadZoneWidth: 300, deadZoneHeight: 150);

//      public MainWindow()
//      {
//         InitializeComponent();

//         // Создаем сущности
//         myHero = new Player(playerX, playerY);
//         GameArea.Children.Add(myHero.VisualShape);

//         currentWeapon = new Pistol();

//         Enemy dummy = new Enemy(400, 300, 50);
//         activeEnemies.Add(dummy);
//         GameArea.Children.Add(dummy.VisualShape);


//         // RoomManager сам спавнит стартовую комнату и дальше сам следит,
//         // что сейчас лежит на Canvas — нам об этом заботиться больше не нужно.
//         roomManager = new RoomManager(GameArea, TestLevel.StartRoom);

//         playerX = roomManager.CurrentRoom.PlayerStartX;
//         playerY = roomManager.CurrentRoom.PlayerStartY;
//         Canvas.SetLeft(Player, playerX);
//         Canvas.SetTop(Player, playerY);

//         camera.SnapTo(playerX, playerY, Player.Width, Player.Height, roomManager.CurrentRoom.Width, roomManager.CurrentRoom.Height);
//         CameraTransform.X = -camera.X;
//         CameraTransform.Y = -camera.Y;

//         // Настройка и запуск таймера
//         gameTimer.Interval = TimeSpan.FromMilliseconds(16);
//         gameTimer.Tick += GameTick;
//         gameTimer.Start();
//      }

//      // ПРОБРОС УПРАВЛЕНИЯ В CORE (Всю работу делает InputManager)
//      private void OnKeyDown(object sender, KeyEventArgs e) => Inputmanager.UpdateKeyState(e.Key, true);
//      private void OnKeyUp(object sender, KeyEventArgs e) => Inputmanager.UpdateKeyState(e.Key, false);

//      private void GameTick(object sender, EventArgs e)
//      {
//         // 1. ИГРОК: Сам читает InputManager и обновляет свою физику
//         myHero.Update(GameArea.Children);
//         myHero.Draw();

//         // 2. ВРАГИ: Каждый враг обновляет свою физику (чтобы не провалиться сквозь пол)
//         foreach (var enemy in activeEnemies)
//         {
//            enemy.UpdatePhysics(GameArea.Children, 0.8, true);
//            enemy.Draw();
//         }

//         // 3. ОРУЖИЕ И СТРЕЛЬБА:
//         currentWeapon.Tick(Inputmanager.Shooting); // Обновляем таймер перезарядки

//         if (Inputmanager.Reloading) currentWeapon.Reload();
//         if (Inputmanager.Shooting) currentWeapon.Attack(GameArea, myHero.X, myHero.Y, myHero.FacingRight, activeBullets);

//         // 4. ПУЛИ И УРОН: Делегируем работу Боевому Менеджеру
//         CombatManager.UpdateBulletsAndHits(activeBullets, activeEnemies, GameArea);

//         // Ограничение экрана — теперь по ширине ТЕКУЩЕЙ комнаты, а не фиксированное число.
//         // Иначе игрок физически не смог бы дойти до двери у правого края большой комнаты.
//         double maxX = roomManager.CurrentRoom.Width - Player.Width;
//         if (playerX < 0) playerX = 0;
//         if (playerX > maxX) playerX = maxX;

//         // Проверка перехода в другую комнату: коснулись ли края комнаты там, где есть дверь.
//         // Если да — RoomManager сам подменяет комнату на Canvas и говорит, куда поставить игрока.
//         Rect currentHitBox = new Rect(playerX, playerY, Player.Width, Player.Height);
//         var transition = roomManager.TryTransition(currentHitBox);
//         if (transition is not null)
//         {
//            (playerX, playerY) = transition.Value;
//            // Новая комната — камера сразу центрируется на игроке, а не "приезжает" из старой
//            camera.SnapTo(playerX, playerY, Player.Width, Player.Height, roomManager.CurrentRoom.Width, roomManager.CurrentRoom.Height);
//         }
//         else
//         {
//            camera.Follow(playerX, playerY, Player.Width, Player.Height, roomManager.CurrentRoom.Width, roomManager.CurrentRoom.Height);
//         }

//         CameraTransform.X = -camera.X;
//         CameraTransform.Y = -camera.Y;

//         Canvas.SetLeft(Player, playerX);
//         Canvas.SetTop(Player, playerY);

//         // 5. ИНТЕРФЕЙС
//         AmmoText.Text = currentWeapon.IsReloading ? "Перезарядка..." : $"{currentWeapon.Ammo} / {currentWeapon.ReserveAmmo}";
//      }
//   }
//}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WPFGame.Core;
using WPFGame.PlayerLogic;
using WPFGame.Weapons;
using WPFGame.Enemies;
using WPFGame.Projectiles;
using WPFGame.Level; // Папка напарника с комнатами

namespace WPFGame
{
   public partial class MainWindow : Window
   {
      private DispatcherTimer gameTimer = new DispatcherTimer();

      // Ваши сущности
      private Player myHero;
      private Weapon currentWeapon;
      private List<Bullet> activeBullets = new List<Bullet>();
      private List<Enemy> activeEnemies = new List<Enemy>();
      private List<System.Windows.Shapes.Line> activeTracers = new List<System.Windows.Shapes.Line>();
      // Фичи напарника
      private RoomManager roomManager;
      private CameraController camera;

      public MainWindow()
      {
         InitializeComponent();

         // Инициализация комнат и камеры (от напарника)
         roomManager = new RoomManager(GameArea, TestLevel.StartRoom);
         camera = new CameraController(viewportWidth: 960, viewportHeight: 540, deadZoneWidth: 300, deadZoneHeight: 150);

         // Инициализация игрока на стартовых координатах комнаты
         myHero = new Player(roomManager.CurrentRoom.PlayerStartX, roomManager.CurrentRoom.PlayerStartY);
         GameArea.Children.Add(myHero.VisualShape);

         camera.SnapTo(myHero.X, myHero.Y, myHero.Width, myHero.Height, roomManager.CurrentRoom.Width, roomManager.CurrentRoom.Height);
         CameraTransform.X = -camera.X;
         CameraTransform.Y = -camera.Y;

         // Нужно для отслеживаня курсора мыши
         Viewport.MouseMove += (s, e) =>
         {
            var position = e.GetPosition(Viewport);
            Inputmanager.MouseX = position.X;
            Inputmanager.MouseY = position.Y;

            Title = $"Mouse: {Inputmanager.MouseX:F0}; {Inputmanager.MouseY:F0}";
         };
         /////////////////////////////////////


         currentWeapon = new Pistol();

         Enemy dummy = new Enemy(400, 300, 100);
         activeEnemies.Add(dummy);
         GameArea.Children.Add(dummy.VisualShape);

         gameTimer.Interval = TimeSpan.FromMilliseconds(16);
         gameTimer.Tick += GameTick;
         gameTimer.Start();
      }

      private void OnKeyDown(object sender, KeyEventArgs e) => Inputmanager.UpdateKeyState(e.Key, true);
      private void OnKeyUp(object sender, KeyEventArgs e) => Inputmanager.UpdateKeyState(e.Key, false);

      private void GameTick(object sender, EventArgs e)
      {
         // 1. ИГРОК: Физика
         // ПЕРЕДАЕМ ИГРОКУ ШИРИНУ КОМНАТЫ, ЧТОБЫ ОН НЕ ВЫБЕЖАЛ ЗА КРАЙ (важно для фичи напарника)
         myHero.Update(GameArea.Children, roomManager.CurrentRoom.Width);
         myHero.Draw();

         // 2. ВРАГИ И ПУЛИ
         foreach (var enemy in activeEnemies)
         {
            enemy.UpdatePhysics(GameArea.Children, 0.8, true);
            enemy.Draw();
         }
         currentWeapon.Tick(Inputmanager.Shooting);
         if (Inputmanager.Reloading) currentWeapon.Reload();
         if (Inputmanager.Shooting) currentWeapon.Attack(GameArea, myHero.X + myHero.Width / 2, myHero.Y + myHero.Height / 2, activeEnemies, GameArea.Children, activeTracers);
         CombatManager.UpdateBulletsAndHits(activeBullets, activeEnemies, GameArea, roomManager.CurrentRoom.Width);

         // 3. ИНТЕРФЕЙС
         AmmoText.Text = currentWeapon.IsReloading ? "Перезарядка..." : $"{currentWeapon.Ammo} / {currentWeapon.ReserveAmmo}";

         // ---------------------------------------------------------
         // 4. КОМНАТЫ И КАМЕРА
         // ---------------------------------------------------------
         Rect currentHitBox = new Rect(myHero.X, myHero.Y, myHero.Width, myHero.Height);
         var transition = roomManager.TryTransition(currentHitBox);

         if (transition is not null)
         {
            // Перешли в новую комнату
            myHero.X = transition.Value.X;
            myHero.Y = transition.Value.Y;
            camera.SnapTo(myHero.X, myHero.Y, myHero.Width, myHero.Height, roomManager.CurrentRoom.Width, roomManager.CurrentRoom.Height);
         }
         else
         {
            // Просто следим за игроком
            camera.Follow(myHero.X, myHero.Y, myHero.Width, myHero.Height, roomManager.CurrentRoom.Width, roomManager.CurrentRoom.Height);
         }

         Title = $"Player: {myHero.X:F0}; {myHero.Y:F0} | Mouse: {Inputmanager.MouseX:F0}; {Inputmanager.MouseY:F0}";
         CameraTransform.X = -camera.X;
         CameraTransform.Y = -camera.Y;
      }
   }
}