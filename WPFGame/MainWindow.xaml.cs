using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WPFGame.Core;
using WPFGame.Enemies;
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

        private const int GeneratedRoomCount =
            8;

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
                TimeSpan.FromMilliseconds(16);

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

        private void ApplyCamera()
        {
            CameraTransform.X =
                -camera.X;

            CameraTransform.Y =
                -camera.Y;
        }
    }
}
