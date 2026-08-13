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

        private Player myHero;
        private Weapon currentWeapon;
        private RoomManager roomManager;
        private CameraController camera;
        private MiniMap miniMap;

        private InteractionZone stationExitZone;
        private InteractionPrompt interactionPrompt;
        private Rectangle stationExitMarker;

        private const int GeneratedRoomCount =
            8;

        private const double InteractionHoldDuration =
            0.8;

        public MainWindow()
        {
            InitializeComponent();

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

            // Миникарта отображает готовый LevelLayout и не зависит от камеры
            miniMap =
                new MiniMap(
                    Viewport,
                    level);

            camera = new CameraController(
                viewportWidth: 960,
                viewportHeight: 540,
                deadZoneWidth: 300,
                deadZoneHeight: 150);

            myHero = new Player(
                roomManager.CurrentOriginX +
                roomManager.CurrentRoom.PlayerStartX,

                roomManager.CurrentOriginY +
                roomManager.CurrentRoom.PlayerStartY);

            GameArea.Children.Add(
                myHero.VisualShape);

            // Игрок отображается поверх обычных тайлов и лестниц
            Panel.SetZIndex(
                myHero.VisualShape,
                ZLayer.Player);

            CreateStationExitTest();

            interactionPrompt =
                new InteractionPrompt(
                    Viewport);

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

            ApplyCamera();

            currentWeapon =
                new Pistol();

            var dummy =
                new Enemy(
                    400,
                    300,
                    100);

            activeEnemies.Add(
                dummy);

            GameArea.Children.Add(
                dummy.VisualShape);

            // Тестовый противник отображается поверх тайлов комнаты
            Panel.SetZIndex(
                dummy.VisualShape,
                ZLayer.Enemies);

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
            // Сохраняет последнюю допустимую позицию перед движением
            double previousPlayerX =
                myHero.X;

            double previousPlayerY =
                myHero.Y;

            // Игрок сначала двигается обычной физикой
            myHero.Update(
                GameArea.Children);

            // Менеджер проверяет двери, границы и форму комнаты
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

            UpdateStationExitTest();

            // Маркер миникарты следует за мировым положением игрока
            miniMap.Update(
                roomManager.CurrentInstance,
                myHero.X,
                myHero.Y,
                myHero.Width,
                myHero.Height);

            foreach (var enemy in activeEnemies)
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

            // Предел полёта снарядов берётся из загруженной области уровня
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
                // После полного перехода камера сразу переключается на новую комнату
                camera.SnapTo(
                    myHero.X,
                    myHero.Y,
                    myHero.Width,
                    myHero.Height,
                    roomManager.CurrentBounds);
            }
            else
            {
                // До перехода камера остаётся ограничена текущей комнатой
                camera.Follow(
                    myHero.X,
                    myHero.Y,
                    myHero.Width,
                    myHero.Height,
                    roomManager.CurrentBounds);
            }

            ApplyCamera();
        }

        // Создаёт временную точку выхода возле стартовой позиции игрока
        private void CreateStationExitTest()
        {
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
                        false
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

        // Обновляет удержание E и показывает HUD-подсказку возле точки выхода
        private void UpdateStationExitTest()
        {
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

        private void ApplyCamera()
        {
            CameraTransform.X =
                -camera.X;

            CameraTransform.Y =
                -camera.Y;
        }
    }
}
