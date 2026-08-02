using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
// Подключаем наши папки:
using WPFGame.Core;
using WPFGame.PlayerLogic;
using WPFGame.Weapons;
using WPFGame.Enemies;
using WPFGame.Projectiles;

namespace WPFGame
{
   public partial class MainWindow : Window
   {
      private DispatcherTimer gameTimer = new DispatcherTimer();

      // Списки и объекты, которые живут на уровне
      private Player myHero;
      private Weapon currentWeapon;
      private List<Bullet> activeBullets = new List<Bullet>();
      private List<Enemy> activeEnemies = new List<Enemy>();

      public MainWindow()
      {
         InitializeComponent();

         // Создаем сущности
         myHero = new Player(100, 100);
         GameArea.Children.Add(myHero.VisualShape);

         currentWeapon = new Pistol();

         Enemy dummy = new Enemy(400, 300, 50);
         activeEnemies.Add(dummy);
         GameArea.Children.Add(dummy.VisualShape);

         // Настройка и запуск таймера
         gameTimer.Interval = TimeSpan.FromMilliseconds(16);
         gameTimer.Tick += GameTick;
         gameTimer.Start();
      }

      // ПРОБРОС УПРАВЛЕНИЯ В CORE (Всю работу делает InputManager)
      private void OnKeyDown(object sender, KeyEventArgs e) => Inputmanager.UpdateKeyState(e.Key, true);
      private void OnKeyUp(object sender, KeyEventArgs e) => Inputmanager.UpdateKeyState(e.Key, false);

      private void GameTick(object sender, EventArgs e)
      {
         // 1. ИГРОК: Сам читает InputManager и обновляет свою физику
         myHero.Update(GameArea.Children);
         myHero.Draw();

         // 2. ВРАГИ: Каждый враг обновляет свою физику (чтобы не провалиться сквозь пол)
         foreach (var enemy in activeEnemies)
         {
            enemy.UpdatePhysics(GameArea.Children, 0.8, true);
            enemy.Draw();
         }

         // 3. ОРУЖИЕ И СТРЕЛЬБА:
         currentWeapon.Tick(Inputmanager.Shooting); // Обновляем таймер перезарядки

         if (Inputmanager.Reloading) currentWeapon.Reload();
         if (Inputmanager.Shooting) currentWeapon.Attack(GameArea, myHero.X, myHero.Y, myHero.FacingRight, activeBullets);

         // 4. ПУЛИ И УРОН: Делегируем работу Боевому Менеджеру
         CombatManager.UpdateBulletsAndHits(activeBullets, activeEnemies, GameArea);

         // 5. ИНТЕРФЕЙС
         AmmoText.Text = currentWeapon.IsReloading ? "Перезарядка..." : $"{currentWeapon.Ammo} / {currentWeapon.ReserveAmmo}";
      }
   }
}