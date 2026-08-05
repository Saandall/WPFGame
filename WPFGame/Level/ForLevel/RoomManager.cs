using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace WPFGame.Level
{
    // Загружает только текущую комнату и одну комнату у активной двери
    public class RoomManager
    {
        private readonly Canvas canvas;

        private List<Rectangle> currentTiles;
        private List<Rectangle> pendingTiles = new();

        private RoomTemplate? pendingRoom;
        private DoorSlot? currentDoorToPending;
        private DoorSlot? pendingDoorToCurrent;

        private double pendingOriginX;
        private double pendingOriginY;

        public RoomTemplate CurrentRoom { get; private set; }

        public double CurrentOriginX { get; private set; }
        public double CurrentOriginY { get; private set; }

        public bool CurrentRoomChanged { get; private set; }

        public bool HasPendingRoom =>
            pendingRoom is not null;

        public Rect CurrentBounds =>
            new(
                CurrentOriginX,
                CurrentOriginY,
                CurrentRoom.Width,
                CurrentRoom.Height);

        public Rect ActiveBounds
        {
            get
            {
                var bounds = CurrentBounds;

                if (pendingRoom is not null)
                {
                    bounds.Union(
                        new Rect(
                            pendingOriginX,
                            pendingOriginY,
                            pendingRoom.Width,
                            pendingRoom.Height));
                }

                return bounds;
            }
        }

        public RoomManager(
            Canvas canvas,
            RoomTemplate startRoom)
        {
            this.canvas = canvas;

            CurrentRoom = startRoom;
            CurrentOriginX = 0;
            CurrentOriginY = 0;

            currentTiles = SpawnRoom(
                CurrentRoom,
                CurrentOriginX,
                CurrentOriginY);
        }

        // Обновляет переход и возвращает допустимую мировую позицию игрока
        public Point UpdatePlayer(
            double previousPlayerX,
            double previousPlayerY,
            double playerX,
            double playerY,
            double playerWidth,
            double playerHeight)
        {
            CurrentRoomChanged = false;

            var playerHitBox = new Rect(
                playerX,
                playerY,
                playerWidth,
                playerHeight);

            if (pendingRoom is null)
            {
                TryBeginTransition(
                    playerHitBox);
            }

            if (pendingRoom is not null &&
                currentDoorToPending is not null)
            {
                if (HasFullyCrossedDoor(
                        playerHitBox,
                        currentDoorToPending))
                {
                    SwapCurrentAndPendingRooms();
                    CurrentRoomChanged = true;

                    return ResolvePlayerInsideLoadedShape(
                        previousPlayerX,
                        previousPlayerY,
                        playerX,
                        playerY,
                        playerWidth,
                        playerHeight);
                }

                if (HasMovedAwayFromDoor(
                        playerHitBox,
                        currentDoorToPending))
                {
                    RemovePendingRoom();
                }
            }

            Point clampedPosition =
                ClampPlayerToCurrentRoom(
                    playerX,
                    playerY,
                    playerWidth,
                    playerHeight);

            return ResolvePlayerInsideLoadedShape(
                previousPlayerX,
                previousPlayerY,
                clampedPosition.X,
                clampedPosition.Y,
                playerWidth,
                playerHeight);
        }

        // Загружает соседа только после касания триггера конкретной двери
        private void TryBeginTransition(
            Rect playerHitBox)
        {
            foreach (var door in CurrentRoom.Doors)
            {
                if (!IsInsideDoorTrigger(
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

                BeginTransition(
                    door,
                    target.Value.Room,
                    target.Value.Door);

                return;
            }
        }

        private void BeginTransition(
            DoorSlot sourceDoor,
            RoomTemplate targetRoom,
            DoorSlot targetDoor)
        {
            if (targetDoor.Direction !=
                sourceDoor.Direction.Opposite())
            {
                throw new InvalidOperationException(
                    $"Двери {CurrentRoom.Id}/{sourceDoor.Id} и " +
                    $"{targetRoom.Id}/{targetDoor.Id} направлены не навстречу друг другу.");
            }

            var targetOrigin =
                CalculateTargetOrigin(
                    sourceDoor,
                    targetDoor);

            pendingRoom = targetRoom;
            pendingOriginX = targetOrigin.X;
            pendingOriginY = targetOrigin.Y;

            currentDoorToPending = sourceDoor;
            pendingDoorToCurrent = targetDoor;

            pendingTiles = SpawnRoom(
                pendingRoom,
                pendingOriginX,
                pendingOriginY);
        }

        // После пересечения новая комната становится текущей без изменения координат игрока
        private void SwapCurrentAndPendingRooms()
        {
            if (pendingRoom is null ||
                currentDoorToPending is null ||
                pendingDoorToCurrent is null)
            {
                return;
            }

            RoomTemplate oldCurrentRoom =
                CurrentRoom;

            double oldCurrentOriginX =
                CurrentOriginX;

            double oldCurrentOriginY =
                CurrentOriginY;

            List<Rectangle> oldCurrentTiles =
                currentTiles;

            DoorSlot oldCurrentDoor =
                currentDoorToPending;

            CurrentRoom = pendingRoom;
            CurrentOriginX = pendingOriginX;
            CurrentOriginY = pendingOriginY;
            currentTiles = pendingTiles;

            pendingRoom = oldCurrentRoom;
            pendingOriginX = oldCurrentOriginX;
            pendingOriginY = oldCurrentOriginY;
            pendingTiles = oldCurrentTiles;

            currentDoorToPending =
                pendingDoorToCurrent;

            pendingDoorToCurrent =
                oldCurrentDoor;
        }

        // Удаляет только визуальные тайлы временной соседней комнаты
        private void RemovePendingRoom()
        {
            foreach (var tile in pendingTiles)
            {
                canvas.Children.Remove(tile);
            }

            pendingTiles.Clear();
            pendingRoom = null;
            currentDoorToPending = null;
            pendingDoorToCurrent = null;
        }

        private List<Rectangle> SpawnRoom(
            RoomTemplate room,
            double originX,
            double originY)
        {
            var spawnedTiles =
                new List<Rectangle>();

            foreach (var tile in room.Tiles)
            {
                var rectangle =
                    RoomSpawner.CreateTile(
                        tile,
                        originX,
                        originY);

                canvas.Children.Add(
                    rectangle);

                spawnedTiles.Add(
                    rectangle);
            }

            return spawnedTiles;
        }

        // Совмещает мировые позиции двух конкретных дверей
        private Point CalculateTargetOrigin(
            DoorSlot sourceDoor,
            DoorSlot targetDoor)
        {
            double sourceBoundary =
                GetDoorBoundary(
                    sourceDoor,
                    CurrentOriginX,
                    CurrentOriginY);

            double targetBoundary =
                GetDoorBoundary(
                    targetDoor,
                    0,
                    0);

            var sourceRange =
                GetDoorRange(
                    sourceDoor,
                    CurrentOriginX,
                    CurrentOriginY);

            var targetRange =
                GetDoorRange(
                    targetDoor,
                    0,
                    0);

            if (sourceDoor.Direction is
                Direction.Left or
                Direction.Right)
            {
                return new Point(
                    sourceBoundary -
                    targetBoundary,

                    sourceRange.Start -
                    targetRange.Start);
            }

            return new Point(
                sourceRange.Start -
                targetRange.Start,

                sourceBoundary -
                targetBoundary);
        }

        private Point ClampPlayerToCurrentRoom(
            double playerX,
            double playerY,
            double playerWidth,
            double playerHeight)
        {
            double minX =
                CurrentBounds.Left;

            double maxX =
                CurrentBounds.Right -
                playerWidth;

            double minY =
                CurrentBounds.Top;

            double maxY =
                CurrentBounds.Bottom -
                playerHeight;

            Direction? openDirection =
                pendingRoom is not null
                    ? currentDoorToPending?.Direction
                    : null;

            if (openDirection != Direction.Left)
            {
                playerX = Math.Max(
                    playerX,
                    minX);
            }

            if (openDirection != Direction.Right)
            {
                playerX = Math.Min(
                    playerX,
                    maxX);
            }

            if (openDirection != Direction.Top)
            {
                playerY = Math.Max(
                    playerY,
                    minY);
            }

            if (openDirection != Direction.Bottom)
            {
                playerY = Math.Min(
                    playerY,
                    maxY);
            }

            return new Point(
                playerX,
                playerY);
        }

        // Не позволяет игроку входить в отсутствующие блоки комнаты
        private Point ResolvePlayerInsideLoadedShape(
            double previousPlayerX,
            double previousPlayerY,
            double playerX,
            double playerY,
            double playerWidth,
            double playerHeight)
        {
            var candidate = new Rect(
                playerX,
                playerY,
                playerWidth,
                playerHeight);

            if (IsHitBoxInsideLoadedShape(candidate))
            {
                return new Point(
                    playerX,
                    playerY);
            }

            // Сначала сохраняется горизонтальная часть движения
            var horizontalOnly = new Rect(
                playerX,
                previousPlayerY,
                playerWidth,
                playerHeight);

            if (IsHitBoxInsideLoadedShape(horizontalOnly))
            {
                return new Point(
                    playerX,
                    previousPlayerY);
            }

            // Затем сохраняется вертикальная часть движения
            var verticalOnly = new Rect(
                previousPlayerX,
                playerY,
                playerWidth,
                playerHeight);

            if (IsHitBoxInsideLoadedShape(verticalOnly))
            {
                return new Point(
                    previousPlayerX,
                    playerY);
            }

            return new Point(
                previousPlayerX,
                previousPlayerY);
        }

        // Проверяет четыре угла хитбокса в объединении загруженных блоков
        private bool IsHitBoxInsideLoadedShape(
            Rect hitBox)
        {
            const double inset = 0.1;

            return IsPointInsideLoadedShape(
                       hitBox.Left + inset,
                       hitBox.Top + inset) &&
                   IsPointInsideLoadedShape(
                       hitBox.Right - inset,
                       hitBox.Top + inset) &&
                   IsPointInsideLoadedShape(
                       hitBox.Left + inset,
                       hitBox.Bottom - inset) &&
                   IsPointInsideLoadedShape(
                       hitBox.Right - inset,
                       hitBox.Bottom - inset);
        }

        // Проверяет точку в текущей или временно загруженной комнате
        private bool IsPointInsideLoadedShape(
            double x,
            double y)
        {
            if (IsPointInsideRoom(
                    x,
                    y,
                    CurrentRoom,
                    CurrentOriginX,
                    CurrentOriginY))
            {
                return true;
            }

            return pendingRoom is not null &&
                   IsPointInsideRoom(
                       x,
                       y,
                       pendingRoom,
                       pendingOriginX,
                       pendingOriginY);
        }

        // Проверяет точку внутри одного из занятых блоков комнаты
        private static bool IsPointInsideRoom(
            double x,
            double y,
            RoomTemplate room,
            double originX,
            double originY)
        {
            foreach (var cell in room.OccupiedCells)
            {
                double left =
                    originX +
                    cell.Col *
                    RoomMetrics.CellWidth;

                double top =
                    originY +
                    cell.Row *
                    RoomMetrics.CellHeight;

                double right =
                    left +
                    RoomMetrics.CellWidth;

                double bottom =
                    top +
                    RoomMetrics.CellHeight;

                if (x >= left &&
                    x <= right &&
                    y >= top &&
                    y <= bottom)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsInsideDoorTrigger(
            Rect playerHitBox,
            DoorSlot door)
        {
            double boundary =
                GetDoorBoundary(
                    door,
                    CurrentOriginX,
                    CurrentOriginY);

            var range =
                GetDoorRange(
                    door,
                    CurrentOriginX,
                    CurrentOriginY);

            return door.Direction switch
            {
                Direction.Left =>
                    playerHitBox.Left <=
                        boundary +
                        RoomMetrics.DoorTriggerDepth &&
                    playerHitBox.Right >=
                        boundary &&
                    Overlaps(
                        playerHitBox.Top,
                        playerHitBox.Bottom,
                        range),

                Direction.Right =>
                    playerHitBox.Right >=
                        boundary -
                        RoomMetrics.DoorTriggerDepth &&
                    playerHitBox.Left <=
                        boundary &&
                    Overlaps(
                        playerHitBox.Top,
                        playerHitBox.Bottom,
                        range),

                Direction.Top =>
                    playerHitBox.Top <=
                        boundary +
                        RoomMetrics.DoorTriggerDepth &&
                    playerHitBox.Bottom >=
                        boundary &&
                    Overlaps(
                        playerHitBox.Left,
                        playerHitBox.Right,
                        range),

                Direction.Bottom =>
                    playerHitBox.Bottom >=
                        boundary -
                        RoomMetrics.DoorTriggerDepth &&
                    playerHitBox.Top <=
                        boundary &&
                    Overlaps(
                        playerHitBox.Left,
                        playerHitBox.Right,
                        range),

                _ => false
            };
        }

        private bool HasFullyCrossedDoor(
            Rect playerHitBox,
            DoorSlot door)
        {
            double boundary =
                GetDoorBoundary(
                    door,
                    CurrentOriginX,
                    CurrentOriginY);

            return door.Direction switch
            {
                Direction.Left =>
                    playerHitBox.Right <=
                    boundary,

                Direction.Right =>
                    playerHitBox.Left >=
                    boundary,

                Direction.Top =>
                    playerHitBox.Bottom <=
                    boundary,

                Direction.Bottom =>
                    playerHitBox.Top >=
                    boundary,

                _ => false
            };
        }

        // После отхода от двери предыдущая комната больше не нужна на Canvas
        private bool HasMovedAwayFromDoor(
            Rect playerHitBox,
            DoorSlot door)
        {
            double boundary =
                GetDoorBoundary(
                    door,
                    CurrentOriginX,
                    CurrentOriginY);

            return door.Direction switch
            {
                Direction.Left =>
                    playerHitBox.Left >
                    boundary +
                    RoomMetrics.DoorTriggerDepth,

                Direction.Right =>
                    playerHitBox.Right <
                    boundary -
                    RoomMetrics.DoorTriggerDepth,

                Direction.Top =>
                    playerHitBox.Top >
                    boundary +
                    RoomMetrics.DoorTriggerDepth,

                Direction.Bottom =>
                    playerHitBox.Bottom <
                    boundary -
                    RoomMetrics.DoorTriggerDepth,

                _ => false
            };
        }

        private static double GetDoorBoundary(
            DoorSlot door,
            double originX,
            double originY)
        {
            return door.Direction switch
            {
                Direction.Left =>
                    originX +
                    door.CellCol *
                    RoomMetrics.CellWidth,

                Direction.Right =>
                    originX +
                    (door.CellCol + 1) *
                    RoomMetrics.CellWidth,

                Direction.Top =>
                    originY +
                    door.CellRow *
                    RoomMetrics.CellHeight,

                Direction.Bottom =>
                    originY +
                    (door.CellRow + 1) *
                    RoomMetrics.CellHeight,

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(door.Direction))
            };
        }

        private static (
            double Start,
            double End)
            GetDoorRange(
                DoorSlot door,
                double originX,
                double originY)
        {
            if (door.Direction is
                Direction.Left or
                Direction.Right)
            {
                double floorY =
                    originY +
                    door.CellRow *
                    RoomMetrics.CellHeight +
                    RoomMetrics.FloorY;

                return (
                    floorY -
                    RoomMetrics.SideDoorHeight,
                    floorY);
            }

            double startX =
                originX +
                door.CellCol *
                RoomMetrics.CellWidth +
                RoomMetrics.TopBottomDoorStartX;

            return (
                startX,
                startX +
                RoomMetrics.TopBottomDoorWidth);
        }

        private static bool Overlaps(
            double firstStart,
            double firstEnd,
            (
                double Start,
                double End
            ) second)
        {
            return firstEnd >= second.Start &&
                   firstStart <= second.End;
        }
    }
}
