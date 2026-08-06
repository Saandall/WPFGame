namespace WPFGame.Level
{
    // Создаёт стабильный ручной уровень через общую модель LevelLayout
    public static class FixedLevelFactory
    {
        public static LevelLayout Create()
        {
            var level =
                new LevelLayout();

            var room1 =
                new RoomInstance(
                    "fixed_01",
                    TestRooms.Room1(),
                    worldCellCol: 0,
                    worldCellRow: 0);

            level.AddRoom(
                room1,
                isStartRoom: true);

            RoomInstance room2 =
                AddConnectedRoom(
                    level,
                    room1,
                    "right_0_0",
                    "fixed_02",
                    TestRooms.Room2(),
                    "left_0_0");

            _ = AddConnectedRoom(
                level,
                room1,
                "top_0_0",
                "fixed_03",
                TestRooms.Room3(),
                "bottom_0_0");

            RoomInstance room4 =
                AddConnectedRoom(
                    level,
                    room2,
                    "right_1_0",
                    "fixed_04",
                    TestRooms.Room4(),
                    "left_0_1");

            RoomInstance room5 =
                AddConnectedRoom(
                    level,
                    room4,
                    "right_0_1",
                    "fixed_05",
                    TestRooms.Room5(),
                    "left_0_1");

            _ = AddConnectedRoom(
                level,
                room5,
                "right_1_1",
                "fixed_06",
                TestRooms.Room6(),
                "left_0_1");

            return level;
        }

        // Размещает шаблон возле двери и сразу создаёт связь
        private static RoomInstance AddConnectedRoom(
            LevelLayout level,
            RoomInstance sourceRoom,
            string sourceDoorId,
            string targetInstanceId,
            RoomTemplate targetTemplate,
            string targetDoorId)
        {
            DoorSlot sourceDoor =
                sourceRoom.GetRequiredDoor(
                    sourceDoorId);

            DoorSlot targetDoor =
                targetTemplate.GetDoor(
                    targetDoorId) ??
                throw new InvalidOperationException(
                    $"Дверь {targetDoorId} не найдена " +
                    $"в шаблоне {targetTemplate.Id}.");

            var targetCell =
                RoomPlacement.CalculateTargetCell(
                    sourceRoom,
                    sourceDoor,
                    targetTemplate,
                    targetDoor);

            if (!level.CanPlaceRoom(
                    targetTemplate,
                    targetCell.Col,
                    targetCell.Row))
            {
                throw new InvalidOperationException(
                    $"Шаблон {targetTemplate.Id} нельзя разместить " +
                    $"в блоке ({targetCell.Col}, {targetCell.Row}).");
            }

            var targetRoom =
                new RoomInstance(
                    targetInstanceId,
                    targetTemplate,
                    targetCell.Col,
                    targetCell.Row);

            level.AddRoom(
                targetRoom);

            level.Connect(
                sourceRoom.Id,
                sourceDoor.Id,
                targetRoom.Id,
                targetDoor.Id);

            return targetRoom;
        }
    }
}
