using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace WPFGame.Level
{
    // Загружает текущий экземпляр комнаты и одного соседа у активной двери
    public class RoomManager
    {
        private readonly Canvas canvas;
        private readonly LevelLayout level;

        private List<Rectangle> currentTiles;
        private List<Rectangle> pendingTiles =
            new();

        private RoomInstance? pendingRoom;
        private DoorSlot? currentDoorToPending;
        private DoorSlot? pendingDoorToCurrent;

        public RoomInstance CurrentInstance
        {
            get;
            private set;
        }

        // Сохраняет прежний удобный доступ к шаблону текущей комнаты
        public RoomTemplate CurrentRoom =>
            CurrentInstance.Template;

        public double CurrentOriginX =>
            CurrentInstance.OriginX;

        public double CurrentOriginY =>
            CurrentInstance.OriginY;

        public bool CurrentRoomChanged
        {
            get;
            private set;
        }

        public bool HasPendingRoom =>
            pendingRoom is not null;

        public Rect CurrentBounds =>
            new(
                CurrentOriginX,
                CurrentOriginY,
                CurrentInstance.Width,
                CurrentInstance.Height);

        public Rect ActiveBounds
        {
            get
            {
                var bounds =
                    CurrentBounds;

                if (pendingRoom is not null)
                {
                    bounds.Union(
                        new Rect(
                            pendingRoom.OriginX,
                            pendingRoom.OriginY,
                            pendingRoom.Width,
                            pendingRoom.Height));
                }

                return bounds;
            }
        }

        public RoomManager(
            Canvas canvas,
            LevelLayout level)
        {
            this.canvas =
                canvas ??
                throw new ArgumentNullException(
                    nameof(canvas));

            this.level =
                level ??
                throw new ArgumentNullException(
                    nameof(level));

            CurrentInstance =
                level.StartRoom;

            currentTiles =
                RoomRenderer.Render(
                    canvas,
                    CurrentInstance.Template,
                    CurrentInstance.OriginX,
                    CurrentInstance.OriginY);
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
            CurrentRoomChanged =
                false;

            var playerHitBox =
                new Rect(
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

                    CurrentRoomChanged =
                        true;

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

        // Удаляет все визуальные тайлы загруженных комнат
        public void Unload()
        {
            RemoveTiles(
                currentTiles);

            RemovePendingRoom();

            CurrentRoomChanged =
                false;
        }

        // Загружает соседа только возле соединённой двери
        private void TryBeginTransition(
            Rect playerHitBox)
        {
            foreach (var door in
                     CurrentRoom.Doors)
            {
                if (!IsInsideDoorTrigger(
                        playerHitBox,
                        door))
                {
                    continue;
                }

                var target =
                    level.GetConnectedRoom(
                        CurrentInstance.Id,
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

        // Подготавливает соседний экземпляр комнаты для перехода
        private void BeginTransition(
            DoorSlot sourceDoor,
            RoomInstance targetRoom,
            DoorSlot targetDoor)
        {
            if (!RoomPlacement.AreDoorsAligned(
                    CurrentInstance,
                    sourceDoor,
                    targetRoom,
                    targetDoor))
            {
                throw new InvalidOperationException(
                    $"Двери {CurrentInstance.Id}/{sourceDoor.Id} и " +
                    $"{targetRoom.Id}/{targetDoor.Id} не совмещены.");
            }

            pendingRoom =
                targetRoom;

            currentDoorToPending =
                sourceDoor;

            pendingDoorToCurrent =
                targetDoor;

            pendingTiles =
                RoomRenderer.Render(
                    canvas,
                    pendingRoom.Template,
                    pendingRoom.OriginX,
                    pendingRoom.OriginY);
        }

        // Меняет текущую и предыдущую комнату без переноса игрока
        private void SwapCurrentAndPendingRooms()
        {
            if (pendingRoom is null ||
                currentDoorToPending is null ||
                pendingDoorToCurrent is null)
            {
                return;
            }

            RoomInstance oldCurrentRoom =
                CurrentInstance;

            List<Rectangle> oldCurrentTiles =
                currentTiles;

            DoorSlot oldCurrentDoor =
                currentDoorToPending;

            CurrentInstance =
                pendingRoom;

            currentTiles =
                pendingTiles;

            pendingRoom =
                oldCurrentRoom;

            pendingTiles =
                oldCurrentTiles;

            currentDoorToPending =
                pendingDoorToCurrent;

            pendingDoorToCurrent =
                oldCurrentDoor;
        }

        // Удаляет визуальные тайлы временной соседней комнаты
        private void RemovePendingRoom()
        {
            RemoveTiles(
                pendingTiles);

            pendingRoom =
                null;

            currentDoorToPending =
                null;

            pendingDoorToCurrent =
                null;
        }

        // Удаляет переданный набор тайлов из игрового Canvas
        private void RemoveTiles(
            List<Rectangle> tiles)
        {
            foreach (var tile in
                     tiles)
            {
                canvas.Children.Remove(
                    tile);
            }

            tiles.Clear();
        }

        // Ограничивает игрока прямоугольником текущей комнаты
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

            if (openDirection !=
                Direction.Left)
            {
                playerX =
                    Math.Max(
                        playerX,
                        minX);
            }

            if (openDirection !=
                Direction.Right)
            {
                playerX =
                    Math.Min(
                        playerX,
                        maxX);
            }

            if (openDirection !=
                Direction.Top)
            {
                playerY =
                    Math.Max(
                        playerY,
                        minY);
            }

            if (openDirection !=
                Direction.Bottom)
            {
                playerY =
                    Math.Min(
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
            var candidate =
                new Rect(
                    playerX,
                    playerY,
                    playerWidth,
                    playerHeight);

            if (IsHitBoxInsideLoadedShape(
                    candidate))
            {
                return new Point(
                    playerX,
                    playerY);
            }

            // Сохраняет горизонтальную часть допустимого движения
            var horizontalOnly =
                new Rect(
                    playerX,
                    previousPlayerY,
                    playerWidth,
                    playerHeight);

            if (IsHitBoxInsideLoadedShape(
                    horizontalOnly))
            {
                return new Point(
                    playerX,
                    previousPlayerY);
            }

            // Сохраняет вертикальную часть допустимого движения
            var verticalOnly =
                new Rect(
                    previousPlayerX,
                    playerY,
                    playerWidth,
                    playerHeight);

            if (IsHitBoxInsideLoadedShape(
                    verticalOnly))
            {
                return new Point(
                    previousPlayerX,
                    playerY);
            }

            return new Point(
                previousPlayerX,
                previousPlayerY);
        }

        // Проверяет четыре угла хитбокса в загруженных блоках
        private bool IsHitBoxInsideLoadedShape(
            Rect hitBox)
        {
            const double inset =
                0.1;

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

        // Проверяет точку в текущем или временном экземпляре комнаты
        private bool IsPointInsideLoadedShape(
            double x,
            double y)
        {
            if (IsPointInsideRoom(
                    x,
                    y,
                    CurrentInstance))
            {
                return true;
            }

            return pendingRoom is not null &&
                   IsPointInsideRoom(
                       x,
                       y,
                       pendingRoom);
        }

        // Проверяет точку внутри одного из занятых блоков экземпляра
        private static bool IsPointInsideRoom(
            double x,
            double y,
            RoomInstance room)
        {
            foreach (var cell in
                     room.Template.OccupiedCells)
            {
                double left =
                    room.OriginX +
                    cell.Col *
                    RoomMetrics.CellWidth;

                double top =
                    room.OriginY +
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

        // Проверяет попадание игрока в область загрузки двери
        private bool IsInsideDoorTrigger(
            Rect playerHitBox,
            DoorSlot door)
        {
            double boundary =
                GetDoorBoundary(
                    door,
                    CurrentInstance);

            var range =
                GetDoorRange(
                    door,
                    CurrentInstance);

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

        // Проверяет полное пересечение мировой границы двери
        private bool HasFullyCrossedDoor(
            Rect playerHitBox,
            DoorSlot door)
        {
            double boundary =
                GetDoorBoundary(
                    door,
                    CurrentInstance);

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

        // Проверяет удаление игрока от входной двери новой комнаты
        private bool HasMovedAwayFromDoor(
            Rect playerHitBox,
            DoorSlot door)
        {
            double boundary =
                GetDoorBoundary(
                    door,
                    CurrentInstance);

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

        // Возвращает мировую координату границы двери
        private static double GetDoorBoundary(
            DoorSlot door,
            RoomInstance room)
        {
            return door.Direction switch
            {
                Direction.Left =>
                    room.OriginX +
                    door.CellCol *
                    RoomMetrics.CellWidth,

                Direction.Right =>
                    room.OriginX +
                    (door.CellCol + 1) *
                    RoomMetrics.CellWidth,

                Direction.Top =>
                    room.OriginY +
                    door.CellRow *
                    RoomMetrics.CellHeight,

                Direction.Bottom =>
                    room.OriginY +
                    (door.CellRow + 1) *
                    RoomMetrics.CellHeight,

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(door.Direction))
            };
        }

        // Возвращает мировой диапазон дверного проёма
        private static (
            double Start,
            double End)
            GetDoorRange(
                DoorSlot door,
                RoomInstance room)
        {
            if (door.Direction is
                Direction.Left or
                Direction.Right)
            {
                double floorY =
                    room.OriginY +
                    door.CellRow *
                    RoomMetrics.CellHeight +
                    RoomMetrics.FloorY;

                return (
                    floorY -
                    RoomMetrics.SideDoorHeight,
                    floorY);
            }

            double startX =
                room.OriginX +
                door.CellCol *
                RoomMetrics.CellWidth +
                RoomMetrics.TopBottomDoorStartX;

            return (
                startX,
                startX +
                RoomMetrics.TopBottomDoorWidth);
        }

        // Проверяет пересечение двух одномерных диапазонов
        private static bool Overlaps(
            double firstStart,
            double firstEnd,
            (
                double Start,
                double End
            ) second)
        {
            return firstEnd >=
                       second.Start &&
                   firstStart <=
                       second.End;
        }
    }
}
