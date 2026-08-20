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
using WPFGame.Weapons;

namespace WPFGame
{
   public partial class MainWindow : Window
   {
      private readonly DispatcherTimer gameTimer = new();

      private readonly List<Enemy> activeEnemies = new();
      private readonly List<Line> activeTracers = new();

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

         // Переводит координаты мыши из viewport в мировые координаты.
         Viewport.MouseMove += (_, e) =>
         {
            Point position = e.GetPosition(Viewport);
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

         myHero = new Player(
            trainScene.PlayerSpawn.X,
            trainScene.PlayerSpawn.Y);

         GameArea.Children.Add(myHero.VisualShape);
         Panel.SetZIndex(myHero.VisualShape, ZLayer.Player);

         camera.SnapTo(
            myHero.X,
            myHero.Y,
            myHero.Width,
            myHero.Height,
            trainScene.Bounds);

         ApplyCamera();

         AmmoText.Visibility = Visibility.Collapsed;

         gameTimer.Interval = TimeSpan.FromMilliseconds(16);
         gameTimer.Tick += GameTick;
         gameTimer.Start();
      }

      private void OnKeyDown(object sender, KeyEventArgs e)
      {
         Inputmanager.UpdateKeyState(e.Key, true);
      }

      private void OnKeyUp(object sender, KeyEventArgs e)
      {
         Inputmanager.UpdateKeyState(e.Key, false);
      }

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

         // Показывает мировые координаты игрока и мыши для отладки.
         Title =
            $"Player: {myHero.X:F0}; {myHero.Y:F0} | " +
            $"Mouse: {Inputmanager.MouseX:F0}; {Inputmanager.MouseY:F0}";
      }

      // Обновляет фиксированную сцену поезда.
      private void UpdateTrainScene()
      {
         myHero.Update(GameArea.Children);
         myHero.Draw();

         if (UpdateInteraction(trainScene.DepartureZone))
         {
            StartNextStation();
            return;
         }

         camera.Follow(
            myHero.X,
            myHero.Y,
            myHero.Width,
            myHero.Height,
            trainScene.Bounds);
      }

      // Выгружает поезд и создаёт следующую процедурную станцию.
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

         System.Diagnostics.Debug.WriteLine(
            $"[GAME FLOW] Station {gameSession.StationNumber} started. " +
            $"Seed: {gameSession.CurrentSeed}.");
      }

      // Обновляет игрока, комнату и боевые объекты станции.
      private void UpdateStationScene()
      {
         if (stationScene is null)
         {
            return;
         }

         double previousPlayerX = myHero.X;
         double previousPlayerY = myHero.Y;

         myHero.Update(GameArea.Children);

         Point correctedPosition = stationScene.UpdatePlayer(
            previousPlayerX,
            previousPlayerY,
            myHero.X,
            myHero.Y,
            myHero.Width,
            myHero.Height);

         myHero.X = correctedPosition.X;
         myHero.Y = correctedPosition.Y;
         myHero.Draw();

         if (UpdateStationExit())
         {
            return;
         }

         stationScene.UpdateMiniMap(
            myHero.X,
            myHero.Y,
            myHero.Width,
            myHero.Height);

         // Обновляет физику противников.
         foreach (Enemy enemy in activeEnemies)
         {
            enemy.UpdatePhysics(
               GameArea.Children,
               0.8,
               true);

            enemy.Draw();
         }

         // Обновляет состояние оружия.
         currentWeapon.Tick(Inputmanager.Shooting);

         if (Inputmanager.Reloading)
         {
            currentWeapon.Reload();
         }

         if (Inputmanager.Shooting)
         {
            currentWeapon.Attack(
               GameArea,
               myHero.X + myHero.Width / 2,
               myHero.Y + myHero.Height / 2,
               activeEnemies,
               GameArea.Children,
               activeTracers);
         }

         // Удаляет трассеры после окончания их времени жизни.
         for (int i = activeTracers.Count - 1; i >= 0; i--)
         {
            Line tracer = activeTracers[i];
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

         AmmoText.Text = currentWeapon.IsReloading
            ? "Перезарядка..."
            : $"{currentWeapon.Ammo} / {currentWeapon.ReserveAmmo}";

         if (stationScene.CurrentRoomChanged)
         {
            camera.SnapTo(
               myHero.X,
               myHero.Y,
               myHero.Width,
               myHero.Height,
               stationScene.CurrentBounds);
         }
         else
         {
            camera.Follow(
               myHero.X,
               myHero.Y,
               myHero.Width,
               myHero.Height,
               stationScene.CurrentBounds);
         }
      }

      // Создаёт процедурную станцию и переносит в неё игрока.
      private void LoadStationScene(int levelSeed)
      {
         stationScene = new StationScene(
            GameArea,
            Viewport,
            levelSeed,
            GeneratedRoomCount,
            InteractionHoldDuration);

         PlacePlayerAt(stationScene.PlayerSpawn);

         camera.SnapTo(
            myHero.X,
            myHero.Y,
            myHero.Width,
            myHero.Height,
            stationScene.CurrentBounds);

         stationScene.UpdateMiniMap(
            myHero.X,
            myHero.Y,
            myHero.Width,
            myHero.Height);

         AmmoText.Visibility = Visibility.Visible;

         CreateStationTestEnemy();
      }

      // Обновляет точку выхода со станции.
      private bool UpdateStationExit()
      {
         if (stationScene is null)
         {
            return false;
         }

         if (!UpdateInteraction(stationScene.ExitZone))
         {
            return false;
         }

         CompleteStation();
         return true;
      }

      // Обновляет механику удержания клавиши в зоне взаимодействия.
      private bool UpdateInteraction(InteractionZone zone)
      {
         bool completed = zone.Update(
            myHero.HitBox,
            Inputmanager.Interacting,
            gameTimer.Interval.TotalSeconds);

         if (zone.IsPlayerInside)
         {
            interactionPrompt.Show(zone);
         }
         else
         {
            interactionPrompt.Hide();
         }

         return completed;
      }

      // Выгружает станцию и возвращает игрока в поезд.
      private void CompleteStation()
      {
         interactionPrompt.Hide();

         ClearStationScene();

         gameSession.EnterTrain();
         UpdateStationInfo();

         trainScene.Load(GameArea);
         PlacePlayerAt(trainScene.PlayerSpawn);

         trainScene.DepartureZone.BlockUntilRelease();

         camera.SnapTo(
            myHero.X,
            myHero.Y,
            myHero.Width,
            myHero.Height,
            trainScene.Bounds);

         AmmoText.Visibility = Visibility.Collapsed;

         System.Diagnostics.Debug.WriteLine(
            $"[GAME FLOW] Station {gameSession.StationNumber} completed. " +
            "Returned to train.");
      }

      // Удаляет станцию и временные боевые объекты.
      private void ClearStationScene()
      {
         stationScene?.Unload();

         foreach (Enemy enemy in activeEnemies)
         {
            GameArea.Children.Remove(enemy.VisualShape);
         }

         foreach (Line tracer in activeTracers)
         {
            GameArea.Children.Remove(tracer);
         }

         activeEnemies.Clear();
         activeTracers.Clear();

         stationScene = null;
      }

      // Создаёт тестового противника на станции.
      private void CreateStationTestEnemy()
      {
         var dummy = new Enemy(400, 300, 100);

         activeEnemies.Add(dummy);
         GameArea.Children.Add(dummy.VisualShape);

         Panel.SetZIndex(
            dummy.VisualShape,
            ZLayer.Enemies);
      }

      // Переносит существующего игрока в указанную точку.
      private void PlacePlayerAt(Point position)
      {
         myHero.X = position.X;
         myHero.Y = position.Y;
         myHero.VelocityY = 0;
         myHero.Draw();
      }

      // Показывает номер станции и seed только на станции.
      private void UpdateStationInfo()
      {
         if (gameSession.CurrentScene != GameSceneType.Station ||
             gameSession.CurrentSeed is null)
         {
            StationInfoText.Visibility = Visibility.Collapsed;
            return;
         }

         StationInfoText.Text =
            $"Station {gameSession.StationNumber} | " +
            $"Seed: {gameSession.CurrentSeed.Value}";

         StationInfoText.Visibility = Visibility.Visible;
      }

      private void ApplyCamera()
      {
         CameraTransform.X = -camera.X;
         CameraTransform.Y = -camera.Y;
      }
   }
}
