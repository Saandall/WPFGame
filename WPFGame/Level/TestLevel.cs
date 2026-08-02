namespace WPFGame.Level
{
    // Временный ручной "граф" уровня из готовых комнат — пока нет генератора.
    // Когда появится LevelGenerator — этот класс, скорее всего, просто исчезнет,
    // а RoomManager будет спрашивать соседнюю комнату у него, а не у TestLevel.
    public static class TestLevel
    {
        private static readonly Dictionary<string, RoomTemplate> rooms = new();
        private static readonly Dictionary<(string RoomId, Direction Direction), string> connections = new();

        static TestLevel()
        {
            var room1 = TestRooms.Room1();
            var room2 = TestRooms.Room2();
            var room3 = TestRooms.Room3();
            var room4 = TestRooms.Room4();

            rooms[room1.Id] = room1;
            rooms[room2.Id] = room2;
            rooms[room3.Id] = room3;
            rooms[room4.Id] = room4;

            connections[(room1.Id, Direction.Right)] = room2.Id;
            connections[(room2.Id, Direction.Left)] = room1.Id;

            connections[(room1.Id, Direction.Top)] = room3.Id;
            connections[(room3.Id, Direction.Bottom)] = room1.Id;

            connections[(room2.Id, Direction.Right)] = room4.Id;
            connections[(room4.Id, Direction.Left)] = room2.Id;
        }

        public static RoomTemplate StartRoom => rooms["room1"];

        // Возвращает соседнюю комнату в заданном направлении, или null,
        // если связь для этой двери ещё не задана.
        public static RoomTemplate? GetNextRoom(string currentRoomId, Direction direction)
        {
            return connections.TryGetValue((currentRoomId, direction), out var nextId)
                ? rooms[nextId]
                : null;
        }
    }
}