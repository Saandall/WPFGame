namespace WPFGame.Level
{
    // Хранит готовое расположение комнат и связи между их дверями
    public class LevelLayout
    {
        private readonly Dictionary<string, RoomInstance> rooms = new();

        private readonly Dictionary<RoomDoorReference, RoomConnection> connectionsByDoor = new();

        private readonly List<RoomConnection> connections = new();

        private readonly HashSet<(int Col, int Row)> occupiedWorldCells = new();

        private RoomInstance? startRoom;

        // можно читать извне какие комнаты и связи существуют
        public IReadOnlyCollection<RoomInstance> Rooms =>
            rooms.Values;

        public IReadOnlyList<RoomConnection> Connections =>
            connections;

        // на случай если попытаться получить стартовую комнату до начала уровня
        public RoomInstance StartRoom =>
            startRoom ??
            throw new InvalidOperationException(
                "Стартовая комната уровня не задана.");

        // Добавляет размещённую комнату и резервирует её мировые блоки
        public void AddRoom(
            RoomInstance room,
            bool isStartRoom = false)
        {
            ArgumentNullException.ThrowIfNull(
                room);

            if (rooms.ContainsKey(room.Id))
            {
                throw new InvalidOperationException(
                    $"Экземпляр комнаты {room.Id} уже добавлен в уровень.");
            }

            var worldCells =
                room.GetOccupiedWorldCells()
                    .ToList();

            // проверка пересечения. если хотя бы одна клетка комнаты занята клеткой другой - исключение
            foreach (var cell in worldCells)
            {
                if (occupiedWorldCells.Contains(cell))
                {
                    throw new InvalidOperationException(
                        $"Экземпляр {room.Id} пересекает занятый блок " +
                        $"({cell.Col}, {cell.Row}).");
                }
            }

            // если второй раз задаём стартовую комнату
            if (isStartRoom &&
                startRoom is not null)
            {
                throw new InvalidOperationException(
                    "Стартовая комната уже задана.");
            }

            // если всё хорошо - добавляем
            rooms.Add(room.Id, room);

            foreach (var cell in worldCells)
            {
                occupiedWorldCells.Add(cell);
            }

            if (isStartRoom)
            {
                startRoom = room;
            }
        }

        // Возвращает экземпляр комнаты по его уникальному ID
        public RoomInstance GetRoom(
            string roomInstanceId)
        {
            if (!rooms.TryGetValue(
                    roomInstanceId,
                    out var room))
            {
                throw new InvalidOperationException(
                    $"Экземпляр комнаты {roomInstanceId} не найден.");
            }

            return room;
        }

        // Проверяет возможность разместить шаблон в указанных глобальных блоках
        public bool CanPlaceRoom(
            RoomTemplate template,
            int worldCellCol,
            int worldCellRow)
        {
            ArgumentNullException.ThrowIfNull(
                template);

            foreach (var cell in
                     template.OccupiedCells)
            {
                var worldCell = (
                    Col: worldCellCol + cell.Col,
                    Row: worldCellRow + cell.Row);

                if (occupiedWorldCells.Contains(
                        worldCell))
                {
                    return false;
                }
            }

            return true;
        }

        // Создаёт проверенную двустороннюю связь между дверями
        public void Connect(
            string firstRoomId,
            string firstDoorId,
            string secondRoomId,
            string secondDoorId)
        {
            RoomInstance firstRoom =
                GetRoom(firstRoomId);

            RoomInstance secondRoom =
                GetRoom(secondRoomId);

            DoorSlot firstDoor =
                firstRoom.GetRequiredDoor(
                    firstDoorId);

            DoorSlot secondDoor =
                secondRoom.GetRequiredDoor(
                    secondDoorId);

            var firstReference =
                new RoomDoorReference(
                    firstRoomId,
                    firstDoorId);

            var secondReference =
                new RoomDoorReference(
                    secondRoomId,
                    secondDoorId);

            if (connectionsByDoor.ContainsKey(
                    firstReference))
            {
                throw new InvalidOperationException(
                    $"Дверь {firstRoomId}/{firstDoorId} уже соединена.");
            }

            if (connectionsByDoor.ContainsKey(
                    secondReference))
            {
                throw new InvalidOperationException(
                    $"Дверь {secondRoomId}/{secondDoorId} уже соединена.");
            }

            if (!RoomPlacement.AreDoorsAligned(
                    firstRoom,
                    firstDoor,
                    secondRoom,
                    secondDoor))
            {
                throw new InvalidOperationException(
                    $"Двери {firstRoomId}/{firstDoorId} и " +
                    $"{secondRoomId}/{secondDoorId} не совмещены в мире.");
            }

            var connection =
                new RoomConnection(
                    firstReference,
                    secondReference);

            connections.Add(
                connection);

            connectionsByDoor.Add(
                firstReference,
                connection);

            connectionsByDoor.Add(
                secondReference,
                connection);
        }

        // Возвращает комнату и дверь с другой стороны перехода
        public (RoomInstance Room, DoorSlot Door)?
            GetConnectedRoom(
                string currentRoomId,
                string currentDoorId)
        {
            var currentReference =
                new RoomDoorReference(
                    currentRoomId,
                    currentDoorId);

            // нельзя подключить одну верь дважды
            if (!connectionsByDoor.TryGetValue(
                    currentReference,
                    out var connection))
            {
                return null;
            }

            RoomDoorReference targetReference =
                connection.GetOther(
                    currentReference);

            RoomInstance targetRoom =
                GetRoom(
                    targetReference.RoomInstanceId);

            DoorSlot targetDoor =
                targetRoom.GetRequiredDoor(
                    targetReference.DoorId);

            return (
                targetRoom,
                targetDoor);
        }
    }
}
