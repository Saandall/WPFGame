using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        // Поля станции пока сохраняются для следующего этапа Train -> Station
        private RoomManager? roomManager;
        private MiniMap? miniMap;
        private InteractionZone? stationExitZone;
        private Rectangle? stationExitMarker;

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

            bool completed =
                trainScene.DepartureZone.Update(
                    myHero.HitBox,
                    Inputmanager.Interacting,
                    gameTimer.Interval.TotalSeconds);

            if (trainScene.DepartureZone.IsPlayerInside)
            {
                interactionPrompt.Show(
                    trainScene.DepartureZone);
            }
            else
            {
                interactionPrompt.Hide();
            }

            if (completed)
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

            gameSession.StartStation();

            LoadStationScene();

            if (stationExitZone is not null)
            {
                // Та же зажатая E не должна сразу запускать новое взаимодействие
                stationExitZone.BlockUntilRelease();
            }

            System.Diagnostics.Debug.WriteLine(
                $"[GAME FLOW] Station {gameSession.StationNumber} started.");
        }

        // Обновляет процедурную станцию
        private void UpdateStationScene()
        {
            if (roomManager is null ||
                miniMap is null ||
                stationExitZone is null)
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
                roomManager.UpdatePlayer(
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

            UpdateStationExit();

            miniMap.Update(
                roomManager.CurrentInstance,
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
                roomManager.ActiveBounds.Right);

            AmmoText.Text =
                currentWeapon.IsReloading
                    ? "Перезарядка..."
                    : $"{currentWeapon.Ammo} / {currentWeapon.ReserveAmmo}";

            if (roomManager.CurrentRoomChanged)
            {
                camera.SnapTo(
                    myHero.X,
                    myHero.Y,
                    myHero.Width,
                    myHero.Height,
                    roomManager.CurrentBounds);
            }
            else
            {
                camera.Follow(
                    myHero.X,
                    myHero.Y,
                    myHero.Width,
                    myHero.Height,
                    roomManager.CurrentBounds);
            }
        }

        // Создаёт новую процедурную станцию и переносит в неё игрока
        private void LoadStationScene()
        {
            int levelSeed =
                Random.Shared.Next();

            LevelLayout level =
                LevelGenerator.Generate(
                    levelSeed,
                    GeneratedRoomCount);

            roomManager =
                new RoomManager(
                    GameArea,
                    level);

            miniMap =
                new MiniMap(
                    Viewport,
                    level);

            myHero.X =
                roomManager.CurrentOriginX +
                roomManager.CurrentRoom.PlayerStartX;

            myHero.Y =
                roomManager.CurrentOriginY +
                roomManager.CurrentRoom.PlayerStartY;

            myHero.VelocityY =
                0;

            myHero.Draw();

            CreateStationExit();

            camera.SnapTo(
                myHero.X,
                myHero.Y,
                myHero.Width,
                myHero.Height,
                roomManager.CurrentBounds);

            miniMap.Update(
                roomManager.CurrentInstance,
                myHero.X,
                myHero.Y,
                myHero.Width,
                myHero.Height);

            AmmoText.Visibility =
                Visibility.Visible;

            CreateStationTestEnemy();
        }

        // Создаёт временную точку возврата возле spawn стартовой комнаты
        private void CreateStationExit()
        {
            if (roomManager is null)
            {
                return;
            }

            Rect bounds =
                new Rect(
                    roomManager.CurrentOriginX +
                    50,

                    roomManager.CurrentOriginY +
                    RoomMetrics.FloorY -
                    70,

                    40,
                    70);

            stationExitZone =
                new InteractionZone(
                    bounds,
                    "Удерживайте E, чтобы покинуть уровень",
                    InteractionHoldDuration);

            stationExitMarker =
                new Rectangle
                {
                    Width =
                        bounds.Width,

                    Height =
                        bounds.Height,

                    Fill =
                        Brushes.Goldenrod,

                    Stroke =
                        Brushes.White,

                    StrokeThickness =
                        2,

                    Opacity =
                        0.45,

                    IsHitTestVisible =
                        false,

                    Tag =
                        "InteractionMarker"
                };

            Canvas.SetLeft(
                stationExitMarker,
                bounds.Left);

            Canvas.SetTop(
                stationExitMarker,
                bounds.Top);

            Panel.SetZIndex(
                stationExitMarker,
                ZLayer.Tiles + 1);

            GameArea.Children.Add(
                stationExitMarker);
        }

        // Обновляет временную точку выхода станции
        private void UpdateStationExit()
        {
            if (stationExitZone is null)
            {
                return;
            }

            bool completed =
                stationExitZone.Update(
                    myHero.HitBox,
                    Inputmanager.Interacting,
                    gameTimer.Interval.TotalSeconds);

            if (stationExitZone.IsPlayerInside)
            {
                interactionPrompt.Show(
                    stationExitZone);
            }
            else
            {
                interactionPrompt.Hide();
            }

            if (completed)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[INTERACTION] Station exit completed.");
            }
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

        private void ApplyCamera()
        {
            CameraTransform.X =
                -camera.X;

            CameraTransform.Y =
                -camera.Y;
        }
    }
}
