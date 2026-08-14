using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private readonly DispatcherTimer gameTimer =
            new();

        private readonly List<Bullet> activeBullets =
            new();

        private readonly List<Enemy> activeEnemies =
            new();

        private readonly GameSession gameSession;
        private readonly TrainScene trainScene;

        private Player myHero;
        private Weapon currentWeapon;
        private CameraController camera;
        private InteractionPrompt interactionPrompt;
        private StationScene? stationScene;

        private const int GeneratedRoomCount =
            8;

        private const double InteractionHoldDuration =
            0.8;

        public MainWindow()
        {
            InitializeComponent();

            gameSession =
                new GameSession();

            trainScene =
                new TrainScene(
                    InteractionHoldDuration);

            camera =
                new CameraController(
                    viewportWidth: 960,
                    viewportHeight: 540,
                    deadZoneWidth: 300,
                    deadZoneHeight: 150);

            interactionPrompt =
                new InteractionPrompt(
                    Viewport);

            currentWeapon =
                new Pistol();

            trainScene.Load(
                GameArea);

            myHero =
                new Player(
                    trainScene.PlayerSpawn.X,
                    trainScene.PlayerSpawn.Y);

            GameArea.Children.Add(
                myHero.VisualShape);

            Panel.SetZIndex(
                myHero.VisualShape,
                ZLayer.Player);

            camera.SnapTo(
                myHero.X,
                myHero.Y,
                myHero.Width,
                myHero.Height,
                trainScene.Bounds);

            ApplyCamera();

            // В поезде боевой HUD пока не нужен
            AmmoText.Visibility =
                Visibility.Collapsed;

            gameTimer.Interval =
                TimeSpan.FromMilliseconds(
                    16);

            gameTimer.Tick +=
                GameTick;

            gameTimer.Start();
        }

        private void OnKeyDown(
            object sender,
            KeyEventArgs e)
        {
            Inputmanager.UpdateKeyState(
                e.Key,
                true);
        }

        private void OnKeyUp(
            object sender,
            KeyEventArgs e)
        {
            Inputmanager.UpdateKeyState(
                e.Key,
                false);
        }

        private void GameTick(
            object? sender,
            EventArgs e)
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
        }

        // Обновляет фиксированную сцену поезда
        private void UpdateTrainScene()
        {
            myHero.Update(
                GameArea.Children);

            myHero.Draw();

            if (UpdateInteraction(
                    trainScene.DepartureZone))
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

        // Выгружает поезд и создаёт следующую процедурную станцию
        private void StartNextStation()
        {
            interactionPrompt.Hide();

            trainScene.Unload(
                GameArea);

            int levelSeed =
                gameSession.StartStation();

            LoadStationScene(
                levelSeed);

            UpdateStationInfo();

            if (stationScene is not null)
            {
                // Та же зажатая E не должна сразу запускать новое взаимодействие
                stationScene.ExitZone.BlockUntilRelease();
            }

            System.Diagnostics.Debug.WriteLine(
                $"[GAME FLOW] Station {gameSession.StationNumber} started. Seed: {gameSession.CurrentSeed}.");
        }

        // Обновляет процедурную станцию
        private void UpdateStationScene()
        {
            if (stationScene is null)
            {
                return;
            }

            double previousPlayerX =
                myHero.X;

            double previousPlayerY =
                myHero.Y;

            myHero.Update(
                GameArea.Children);

            Point correctedPosition =
                stationScene.UpdatePlayer(
                    previousPlayerX,
                    previousPlayerY,
                    myHero.X,
                    myHero.Y,
                    myHero.Width,
                    myHero.Height);

            myHero.X =
                correctedPosition.X;

            myHero.Y =
                correctedPosition.Y;

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

            foreach (var enemy in
                     activeEnemies)
            {
                enemy.UpdatePhysics(
                    GameArea.Children,
                    0.8,
                    true);

                enemy.Draw();
            }

            currentWeapon.Tick(
                Inputmanager.Shooting);

            if (Inputmanager.Reloading)
            {
                currentWeapon.Reload();
            }

            if (Inputmanager.Shooting)
            {
                currentWeapon.Attack(
                    GameArea,
                    myHero.X,
                    myHero.Y,
                    myHero.FacingRight,
                    activeBullets);
            }

            CombatManager.UpdateBulletsAndHits(
                activeBullets,
                activeEnemies,
                GameArea,
                stationScene.ActiveBounds.Right);

            AmmoText.Text =
                currentWeapon.IsReloading
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

        // Создаёт новую процедурную станцию и переносит в неё игрока
        private void LoadStationScene(
            int levelSeed)
        {
            stationScene =
                new StationScene(
                    GameArea,
                    Viewport,
                    levelSeed,
                    GeneratedRoomCount,
                    InteractionHoldDuration);

            PlacePlayerAt(
                stationScene.PlayerSpawn);

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

            AmmoText.Visibility =
                Visibility.Visible;

            CreateStationTestEnemy();
        }

        // Обновляет точку выхода и сообщает, была ли станция завершена
        private bool UpdateStationExit()
        {
            if (stationScene is null)
            {
                return false;
            }

            if (!UpdateInteraction(
                    stationScene.ExitZone))
            {
                return false;
            }

            CompleteStation();

            return true;
        }

        // Обновляет общую механику удержания клавиши в зоне взаимодействия
        private bool UpdateInteraction(
            InteractionZone zone)
        {
            bool completed =
                zone.Update(
                    myHero.HitBox,
                    Inputmanager.Interacting,
                    gameTimer.Interval.TotalSeconds);

            if (zone.IsPlayerInside)
            {
                interactionPrompt.Show(
                    zone);
            }
            else
            {
                interactionPrompt.Hide();
            }

            return completed;
        }

        // Полностью выгружает процедурную станцию и возвращает игрока в поезд
        private void CompleteStation()
        {
            interactionPrompt.Hide();

            ClearStationScene();

            gameSession.EnterTrain();

            UpdateStationInfo();

            trainScene.Load(
                GameArea);

            PlacePlayerAt(
                trainScene.PlayerSpawn);

            // Зажатая на выходе E не должна сразу активировать отправление поезда
            trainScene.DepartureZone.BlockUntilRelease();

            camera.SnapTo(
                myHero.X,
                myHero.Y,
                myHero.Width,
                myHero.Height,
                trainScene.Bounds);

            AmmoText.Visibility =
                Visibility.Collapsed;

            System.Diagnostics.Debug.WriteLine(
                $"[GAME FLOW] Station {gameSession.StationNumber} completed. Returned to train.");
        }

        // Удаляет станцию и временные combat-объекты
        private void ClearStationScene()
        {
            stationScene?.Unload();

            foreach (var enemy in
                     activeEnemies)
            {
                GameArea.Children.Remove(
                    enemy.VisualShape);
            }

            foreach (var bullet in
                     activeBullets)
            {
                GameArea.Children.Remove(
                    bullet.VisualShape);
            }

            activeBullets.Clear();
            activeEnemies.Clear();

            stationScene =
                null;
        }

        // Сохраняет существующего тестового противника только для процедурной станции
        private void CreateStationTestEnemy()
        {
            var dummy =
                new Enemy(
                    400,
                    300,
                    100);

            activeEnemies.Add(
                dummy);

            GameArea.Children.Add(
                dummy.VisualShape);

            Panel.SetZIndex(
                dummy.VisualShape,
                ZLayer.Enemies);
        }

        // Помещает существующего игрока в spawn новой сцены
        private void PlacePlayerAt(
            Point position)
        {
            myHero.X =
                position.X;

            myHero.Y =
                position.Y;

            myHero.VelocityY =
                0;

            myHero.Draw();
        }

        // Показывает номер и seed только для активной процедурной станции
        private void UpdateStationInfo()
        {
            if (gameSession.CurrentScene !=
                    GameSceneType.Station ||
                gameSession.CurrentSeed is null)
            {
                StationInfoText.Visibility =
                    Visibility.Collapsed;

                return;
            }

            StationInfoText.Text =
                $"Station {gameSession.StationNumber} | Seed: {gameSession.CurrentSeed.Value}";

            StationInfoText.Visibility =
                Visibility.Visible;
        }

        private void ApplyCamera()
        {
            CameraTransform.X =
                -camera.X;

            CameraTransform.Y =
                -camera.Y;
        }
    }
}
