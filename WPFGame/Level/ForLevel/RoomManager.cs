using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace WPFGame.Level
{
    // Отвечает за то, какая комната сейчас на Canvas, и за переход в соседнюю
    // комнату, когда игрок касается края текущей со стороны, где есть дверь.
    // Ничего не знает про физику игрока (гравитация, лестницы) — только про то,
    // где начинается и заканчивается комната.
    public class RoomManager
    {
        private readonly Canvas canvas;
        private readonly List<Rectangle> spawnedTiles = new();

        public RoomTemplate CurrentRoom { get; private set; }

        public RoomManager(Canvas canvas, RoomTemplate startRoom)
        {
            this.canvas = canvas;
            CurrentRoom = startRoom;
            SpawnCurrentRoom();
        }

        // Вызывается каждый кадр из GameTick с актуальным хитбоксом игрока.
        // Возвращает новую позицию игрока, ЕСЛИ произошёл переход, иначе null.
        public (double X, double Y)? TryTransition(Rect playerHitBox)
        {
            foreach (var direction in CurrentRoom.Doors.Keys)
            {
                if (!IsTouchingDoor(playerHitBox, direction)) continue;

                var nextRoom = TestLevel.GetNextRoom(CurrentRoom.Id, direction);
                if (nextRoom is null) continue; // дверь есть, а соседняя комната пока не задана

                return LoadRoom(nextRoom, direction.Opposite());
            }

            return null;
        }

        private (double X, double Y) LoadRoom(RoomTemplate room, Direction enteredFrom)
        {
            ClearCurrentRoom();
            CurrentRoom = room;
            SpawnCurrentRoom();

            return room.EntryPoints.TryGetValue(enteredFrom, out var point)
                ? point
                : (room.PlayerStartX, room.PlayerStartY); // на случай, если точка входа не задана
        }

        private void SpawnCurrentRoom()
        {
            foreach (var tile in CurrentRoom.Tiles)
            {
                var rect = RoomSpawner.CreateTile(tile);
                canvas.Children.Add(rect);
                spawnedTiles.Add(rect); // запоминаем — это наши, их и будем убирать
            }
        }

        private void ClearCurrentRoom()
        {
            foreach (var rect in spawnedTiles)
            {
                canvas.Children.Remove(rect);
            }

            spawnedTiles.Clear();
        }

        // Касание края комнаты + попадание в диапазон конкретной двери (не вся стена)
        private bool IsTouchingDoor(Rect playerHitBox, Direction direction)
        {
            if (!CurrentRoom.Doors.TryGetValue(direction, out var zone)) return false;

            return direction switch
            {
                Direction.Left => playerHitBox.Left <= 0 && Overlaps(playerHitBox.Top, playerHitBox.Bottom, zone),
                Direction.Right => playerHitBox.Right >= CurrentRoom.Width && Overlaps(playerHitBox.Top, playerHitBox.Bottom, zone),
                Direction.Top => playerHitBox.Top <= 0 && Overlaps(playerHitBox.Left, playerHitBox.Right, zone),
                Direction.Bottom => playerHitBox.Bottom >= CurrentRoom.Height && Overlaps(playerHitBox.Left, playerHitBox.Right, zone),
                _ => false
            };
        }

        // Пересекается ли отрезок [a,b] (край хитбокса игрока) с диапазоном двери
        private static bool Overlaps(double a, double b, (double Start, double End) zone) => b >= zone.Start && a <= zone.End;
    }
}