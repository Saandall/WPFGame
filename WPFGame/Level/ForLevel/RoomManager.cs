using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace WPFGame.Level
{
    // Управляет текущей комнатой и временным телепортационным переходом
    public class RoomManager
    {
        private readonly Canvas canvas;
        private readonly List<Rectangle> spawnedTiles = new();

        public RoomTemplate CurrentRoom { get; private set; }

        public RoomManager(
            Canvas canvas,
            RoomTemplate startRoom)
        {
            this.canvas = canvas;
            CurrentRoom = startRoom;
            SpawnCurrentRoom();
        }

        // Пока сохраняет старое поведение: заменяет комнату и возвращает новую позицию игрока
        public (double X, double Y)?
            TryTransition(Rect playerHitBox)
        {
            foreach (var door in CurrentRoom.Doors)
            {
                if (!IsTouchingDoor(
                        playerHitBox,
                        door))
                {
                    continue;
                }

                var target = TestLevel.GetNextRoom(
                    CurrentRoom.Id,
                    door.Id);

                if (target is null)
                {
                    continue;
                }

                return LoadRoom(
                    target.Value.Room,
                    target.Value.Door,
                    playerHitBox.Width,
                    playerHitBox.Height);
            }

            return null;
        }

        private (double X, double Y) LoadRoom(
            RoomTemplate room,
            DoorSlot enteredDoor,
            double playerWidth,
            double playerHeight)
        {
            ClearCurrentRoom();
            CurrentRoom = room;
            SpawnCurrentRoom();

            return GetEntryPoint(
                enteredDoor,
                playerWidth,
                playerHeight);
        }

        private void SpawnCurrentRoom()
        {
            foreach (var tile in CurrentRoom.Tiles)
            {
                var rectangle =
                    RoomSpawner.CreateTile(tile);

                canvas.Children.Add(rectangle);
                spawnedTiles.Add(rectangle);
            }
        }

        private void ClearCurrentRoom()
        {
            foreach (var rectangle in spawnedTiles)
            {
                canvas.Children.Remove(rectangle);
            }

            spawnedTiles.Clear();
        }

        // Проверяет пересечение игрока с линией конкретной двери
        private static bool IsTouchingDoor(
            Rect playerHitBox,
            DoorSlot door)
        {
            var range = GetDoorRange(door);
            double boundary =
                GetDoorBoundary(door);

            return door.Direction switch
            {
                Direction.Left =>
                    playerHitBox.Left <= boundary &&
                    playerHitBox.Right >= boundary &&
                    Overlaps(
                        playerHitBox.Top,
                        playerHitBox.Bottom,
                        range),

                Direction.Right =>
                    playerHitBox.Left <= boundary &&
                    playerHitBox.Right >= boundary &&
                    Overlaps(
                        playerHitBox.Top,
                        playerHitBox.Bottom,
                        range),

                Direction.Top =>
                    playerHitBox.Top <= boundary &&
                    playerHitBox.Bottom >= boundary &&
                    Overlaps(
                        playerHitBox.Left,
                        playerHitBox.Right,
                        range),

                Direction.Bottom =>
                    playerHitBox.Top <= boundary &&
                    playerHitBox.Bottom >= boundary &&
                    Overlaps(
                        playerHitBox.Left,
                        playerHitBox.Right,
                        range),

                _ => false
            };
        }

        // Вычисляет диапазон двери относительно её блока
        private static (double Start, double End)
            GetDoorRange(DoorSlot door)
        {
            if (door.Direction is
                Direction.Left or Direction.Right)
            {
                double blockFloorY =
                    door.CellRow *
                    RoomMetrics.CellHeight +
                    RoomMetrics.FloorY;

                return (
                    blockFloorY -
                    RoomMetrics.SideDoorHeight,
                    blockFloorY);
            }

            double start =
                door.CellCol *
                RoomMetrics.CellWidth +
                RoomMetrics.TopBottomDoorStartX;

            return (
                start,
                start +
                RoomMetrics.TopBottomDoorWidth);
        }

        // Вычисляет координату внешней стороны выбранного блока
        private static double GetDoorBoundary(
            DoorSlot door)
        {
            return door.Direction switch
            {
                Direction.Left =>
                    door.CellCol *
                    RoomMetrics.CellWidth,

                Direction.Right =>
                    (door.CellCol + 1) *
                    RoomMetrics.CellWidth,

                Direction.Top =>
                    door.CellRow *
                    RoomMetrics.CellHeight,

                Direction.Bottom =>
                    (door.CellRow + 1) *
                    RoomMetrics.CellHeight,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(door.Direction))
            };
        }

        // Временная точка появления, пока переход ещё не стал бесшовным
        private static (double X, double Y)
            GetEntryPoint(
                DoorSlot door,
                double playerWidth,
                double playerHeight)
        {
            var range = GetDoorRange(door);
            double boundary =
                GetDoorBoundary(door);

            double centeredX =
                range.Start +
                (range.End -
                 range.Start -
                 playerWidth) / 2;

            double standingY =
                range.End -
                playerHeight;

            return door.Direction switch
            {
                Direction.Left =>
                    (
                        boundary +
                        RoomMetrics.DoorTriggerDepth,
                        standingY
                    ),

                Direction.Right =>
                    (
                        boundary -
                        RoomMetrics.DoorTriggerDepth -
                        playerWidth,
                        standingY
                    ),

                Direction.Top =>
                    (
                        centeredX,
                        boundary +
                        RoomMetrics.DoorTriggerDepth
                    ),

                Direction.Bottom =>
                    (
                        centeredX,
                        boundary -
                        RoomMetrics.DoorTriggerDepth -
                        playerHeight
                    ),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(door.Direction))
            };
        }

        private static bool Overlaps(
            double firstStart,
            double firstEnd,
            (double Start, double End) second)
        {
            return firstEnd >= second.Start &&
                   firstStart <= second.End;
        }
    }
}
