using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPFGame.Core;
using WPFGame.Enemies;
using WPFGame.GameFlow;
using WPFGame.Level;
using WPFGame.PlayerLogic;
using WPFGame.Projectiles;
using WPFGame.Weapons;

namespace WPFGame
{
   public partial class MainWindow : Window
   {
      private readonly DispatcherTimer gameTimer = new();

      // Списки активных объектов на уровне
      private readonly List<Bullet> activeBullets = new();
      private readonly List<Enemy> activeEnemies = new();
      private readonly List<Line> activeTracers = new(); // ВАША ФИЧА: Вспышки выстрелов

      private readonly GameSession gameSession;
      private readonly TrainScene trainScene;

      private Player myHero;
      private Weapon currentWeapon;
      private CameraController camera;
      private InteractionPrompt interactionPrompt;
      private StationScene? stationScene;

      private const int GeneratedRoomCount = 8;
      private const double InteractionHoldDuration = 0.8;

      public MainWindow()
      {
         InitializeComponent();

         // ВАША ФИЧА: Отслеживание мыши с учетом положения камеры
         Viewport.MouseMove += (s, e) =>
         {
            var position = e.GetPosition(Viewport);
            Inputmanager.MouseX = position.X + camera.X;
            Inputmanager.MouseY = position.Y + camera.Y;
         };

         gameSession = new GameSession();
         trainScene = new TrainScene(InteractionHoldDuration);

         camera = new CameraController(
                 viewportWidth: 960,
                 viewportHeight: 540,
                 deadZoneWidth: 300,
                 deadZoneHeight: 150);

         interactionPrompt = new InteractionPrompt(Viewport);

         currentWeapon = new Pistol();

         trainScene.Load(GameArea);

         myHero = new Player(trainScene.PlayerSpawn.X, trainScene.PlayerSpawn.Y);
         GameArea.Children.Add(myHero.VisualShape);
         Panel.SetZIndex(myHero.VisualShape, ZLayer.Player);

         camera.SnapTo(myHero.X, myHero.Y, myHero.Width, myHero.Height, trainScene.Bounds);
         ApplyCamera();

         // В поезде боевой HUD пока не нужен
         AmmoText.Visibility = Visibility.Collapsed;

         gameTimer.Interval = TimeSpan.FromMilliseconds(16);
         gameTimer.Tick += GameTick;
         gameTimer.Start();
      }

      private void OnKeyDown(object sender, KeyEventArgs e) => Inputmanager.UpdateKeyState(e.Key, true);

      private void OnKeyUp(object sender, KeyEventArgs e) => Inputmanager.UpdateKeyState(e.Key, false);

      private void GameTick(object? sender, EventArgs e)
      {
         switch (gameSession.CurrentScene)
         {
            case GameSceneType.Train:
               UpdateTrainScene();
               break;

            case GameSceneType.Station:
               UpdateStationScene();
               break;

            default:
               throw new ArgumentOutOfRangeException();
         }

         ApplyCamera();

         // ВАША ФИЧА: Вывод координат в заголовок окна для дебага
         Title = $"Player: {myHero.X:F0}; {myHero.Y:F0} | Mouse: {Inputmanager.MouseX:F0}; {Inputmanager.MouseY:F0}";
      }

      // Обновляет фиксированную сцену поезда
      private void UpdateTrainScene()
      {
         myHero.Update(GameArea.Children, trainScene.Bounds.Right);
         myHero.Draw();

         if (UpdateInteraction(trainScene.DepartureZone))
         {
            StartNextStation();
            return;
         }

         camera.Follow(myHero.X, myHero.Y, myHero.Width, myHero.Height, trainScene.Bounds);
      }

      // Выгружает поезд и создаёт следующую процедурную станцию
      private void StartNextStation()
      {
         interactionPrompt.Hide();
         trainScene.Unload(GameArea);

         int levelSeed = gameSession.StartStation();
         LoadStationScene(levelSeed);
         UpdateStationInfo();

         if (stationScene is not null)
         {
            stationScene.ExitZone.BlockUntilRelease();
         }

         System.Diagnostics.Debug.WriteLine($"[GAME FLOW] Station {gameSession.StationNumber} started. Seed: {gameSession.CurrentSeed}.");
      }

      // Обновляет процедурную станцию (ГДЕ ПРОИСХОДИТ БОЙ)
      private void UpdateStationScene()
      {
         if (stationScene is null) return;

         myHero.Update(GameArea.Children, stationScene.ActiveBounds.Right);

         Point correctedPosition = stationScene.UpdatePlayer(myHero.X, myHero.Y, myHero.X, myHero.Y, myHero.Width, myHero.Height);
         myHero.X = correctedPosition.X;
         myHero.Y = correctedPosition.Y;
         myHero.Draw();

         if (UpdateStationExit()) return;

         stationScene.UpdateMiniMap(myHero.X, myHero.Y, myHero.Width, myHero.Height);

         // Физика врагов
         foreach (var enemy in activeEnemies)
         {
            enemy.UpdatePhysics(GameArea.Children, 0.8, true);
            enemy.Draw();
         }

         // ==========================================
         // ВАША ФИЧА: ОРУЖИЕ, HITSCAN И ТРАССЕРЫ
         // ==========================================
         currentWeapon.Tick(Inputmanager.Shooting);

         if (Inputmanager.Reloading)
            currentWeapon.Reload();

         if (Inputmanager.Shooting)
         {
            currentWeapon.Attack(GameArea, myHero.X + myHero.Width / 2, myHero.Y + myHero.Height / 2, activeEnemies, GameArea.Children, activeTracers);
         }

         // Уменьшаем время жизни трассеров, чтобы далее удалить с экрана
         for (int i = activeTracers.Count - 1; i >= 0; i--)
         {
            var tracer = activeTracers[i];
            int framesLeft = (int)tracer.Tag;
            framesLeft--;

            if (framesLeft <= 0)
            {
               GameArea.Children.Remove(tracer);
               activeTracers.RemoveAt(i);
            }
            else
            {
               tracer.Tag = framesLeft;
            }
         }

         // Старые пули (если еще используются)
         CombatManager.UpdateBulletsAndHits(activeBullets, activeEnemies, GameArea, stationScene.ActiveBounds.Right);

         // UI Патронов
         AmmoText.Text = currentWeapon.IsReloading ? "Перезарядка..." : $"{currentWeapon.Ammo} / {currentWeapon.ReserveAmmo}";

         // Обновление камеры
         if (stationScene.CurrentRoomChanged)
         {
            camera.SnapTo(myHero.X, myHero.Y, myHero.Width, myHero.Height, stationScene.CurrentBounds);
         }
         else
         {
            camera.Follow(myHero.X, myHero.Y, myHero.Width, myHero.Height, stationScene.CurrentBounds);
         }
      }

      private void LoadStationScene(int levelSeed)
      {
         stationScene = new StationScene(GameArea, Viewport, levelSeed, GeneratedRoomCount, InteractionHoldDuration);
         PlacePlayerAt(stationScene.PlayerSpawn);
         camera.SnapTo(myHero.X, myHero.Y, myHero.Width, myHero.Height, stationScene.CurrentBounds);
         stationScene.UpdateMiniMap(myHero.X, myHero.Y, myHero.Width, myHero.Height);
         AmmoText.Visibility = Visibility.Visible;
         CreateStationTestEnemy();
      }

      private bool UpdateStationExit()
      {
         if (stationScene is null) return false;
         if (!UpdateInteraction(stationScene.ExitZone)) return false;

         CompleteStation();
         return true;
      }

      private bool UpdateInteraction(InteractionZone zone)
      {
         bool completed = zone.Update(myHero.HitBox, Inputmanager.Interacting, gameTimer.Interval.TotalSeconds);
         if (zone.IsPlayerInside) interactionPrompt.Show(zone);
         else interactionPrompt.Hide();
         return completed;
      }

      private void CompleteStation()
      {
         interactionPrompt.Hide();
         ClearStationScene();
         gameSession.EnterTrain();
         UpdateStationInfo();
         trainScene.Load(GameArea);
         PlacePlayerAt(trainScene.PlayerSpawn);
         trainScene.DepartureZone.BlockUntilRelease();
         camera.SnapTo(myHero.X, myHero.Y, myHero.Width, myHero.Height, trainScene.Bounds);
         AmmoText.Visibility = Visibility.Collapsed;
         System.Diagnostics.Debug.WriteLine($"[GAME FLOW] Station {gameSession.StationNumber} completed. Returned to train.");
      }

      // Выгружает уровень и очищает боевой мусор
      private void ClearStationScene()
      {
         stationScene?.Unload();

         foreach (var enemy in activeEnemies) GameArea.Children.Remove(enemy.VisualShape);
         foreach (var bullet in activeBullets) GameArea.Children.Remove(bullet.VisualShape);

         // ВАША ФИЧА: Очистка трассеров при выходе с уровня
         foreach (var tracer in activeTracers) GameArea.Children.Remove(tracer);

         activeBullets.Clear();
         activeEnemies.Clear();
         activeTracers.Clear();

         stationScene = null;
      }

      private void CreateStationTestEnemy()
      {
         var dummy = new Enemy(400, 300, 100);
         activeEnemies.Add(dummy);
         GameArea.Children.Add(dummy.VisualShape);
         Panel.SetZIndex(dummy.VisualShape, ZLayer.Enemies);
      }

      private void PlacePlayerAt(Point position)
      {
         myHero.X = position.X;
         myHero.Y = position.Y;
         myHero.VelocityY = 0;
         myHero.Draw();
      }

      private void UpdateStationInfo()
      {
         if (gameSession.CurrentScene != GameSceneType.Station || gameSession.CurrentSeed is null)
         {
            StationInfoText.Visibility = Visibility.Collapsed;
            return;
         }
         StationInfoText.Text = $"Station {gameSession.StationNumber} | Seed: {gameSession.CurrentSeed.Value}";
         StationInfoText.Visibility = Visibility.Visible;
      }

      private void ApplyCamera()
      {
         CameraTransform.X = -camera.X;
         CameraTransform.Y = -camera.Y;
      }
   }
}