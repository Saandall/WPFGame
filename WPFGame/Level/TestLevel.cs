namespace WPFGame.Level
{
    // Временный ручной граф уровня из готовых комнат
    public static class TestLevel
    {
        private static readonly Dictionary<string, RoomTemplate> rooms = new();

        private static readonly Dictionary<
            (string RoomId, string DoorId),
            (string RoomId, string DoorId)> connections = new();

        static TestLevel()
        {
            var room1 = TestRooms.Room1();
            var room2 = TestRooms.Room2();
            var room3 = TestRooms.Room3();
            var room4 = TestRooms.Room4();
            var room5 = TestRooms.Room5();
            var room6 = TestRooms.Room6();

            rooms[room1.Id] = room1;
            rooms[room2.Id] = room2;
            rooms[room3.Id] = room3;
            rooms[room4.Id] = room4;
            rooms[room5.Id] = room5;
            rooms[room6.Id] = room6;

            Connect(
                room1,
                "right_0_0",
                room2,
                "left_0_0");

            Connect(
                room1,
                "top_0_0",
                room3,
                "bottom_0_0");

            Connect(
                room2,
                "right_1_0",
                room4,
                "left_0_1");

            Connect(
                room4,
                "right_0_1",
                room5,
                "left_0_1");

            Connect(
                room5,
                "right_1_1",
                room6,
                "left_0_1");
        }

        public static RoomTemplate StartRoom =>
            rooms["room1"];

        // Возвращает соседнюю комнату и её входную дверь
        public static (RoomTemplate Room, DoorSlot Door)?
            GetNextRoom(
                string currentRoomId,
                string currentDoorId)
        {
            if (!connections.TryGetValue(
                    (currentRoomId, currentDoorId),
                    out var target))
            {
                return null;
            }

            if (!rooms.TryGetValue(
                    target.RoomId,
                    out var room))
            {
                throw new InvalidOperationException(
                    $"Комната {target.RoomId} не найдена.");
            }

            var door = room.GetDoor(target.DoorId);

            if (door is null)
            {
                throw new InvalidOperationException(
                    $"Дверь {target.DoorId} не найдена в комнате {room.Id}.");
            }

            return (room, door);
        }

        // Создаёт двустороннюю связь между двумя дверями
        private static void Connect(
            RoomTemplate firstRoom,
            string firstDoorId,
            RoomTemplate secondRoom,
            string secondDoorId)
        {
            ValidateDoor(
                firstRoom,
                firstDoorId);

            ValidateDoor(
                secondRoom,
                secondDoorId);

            connections[
                (firstRoom.Id, firstDoorId)] =
                (secondRoom.Id, secondDoorId);

            connections[
                (secondRoom.Id, secondDoorId)] =
                (firstRoom.Id, firstDoorId);
        }

        private static void ValidateDoor(
            RoomTemplate room,
            string doorId)
        {
            if (room.GetDoor(doorId) is null)
            {
                throw new InvalidOperationException(
                    $"Дверь {doorId} не найдена в комнате {room.Id}.");
            }
        }
    }
}
