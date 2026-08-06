namespace WPFGame.Level
{
    // Рассчитывает положение комнаты по совмещаемым дверям
    public static class RoomPlacement
    {
        public static (int Col, int Row)
            CalculateTargetCell(
                RoomInstance sourceRoom,
                DoorSlot sourceDoor,
                RoomTemplate targetTemplate,
                DoorSlot targetDoor)
        {
            ArgumentNullException.ThrowIfNull(
                sourceRoom);

            ArgumentNullException.ThrowIfNull(
                sourceDoor);

            ArgumentNullException.ThrowIfNull(
                targetTemplate);

            ArgumentNullException.ThrowIfNull(
                targetDoor);

            ValidateDoorBelongsToTemplate(
                sourceRoom.Template,
                sourceDoor);

            ValidateDoorBelongsToTemplate(
                targetTemplate,
                targetDoor);

            ValidateOppositeDirections(
                sourceDoor,
                targetDoor);

            if (sourceDoor.Direction is
                Direction.Left or
                Direction.Right)
            {
                int sourceBoundaryCol =
                    GetVerticalBoundaryCol(
                        sourceRoom.WorldCellCol,
                        sourceDoor);

                int targetBoundaryCol =
                    GetVerticalBoundaryCol(
                        0,
                        targetDoor);

                int targetWorldRow =
                    sourceRoom.WorldCellRow +
                    sourceDoor.CellRow -
                    targetDoor.CellRow;

                return (
                    sourceBoundaryCol -
                        targetBoundaryCol,
                    targetWorldRow);
            }

            int sourceBoundaryRow =
                GetHorizontalBoundaryRow(
                    sourceRoom.WorldCellRow,
                    sourceDoor);

            int targetBoundaryRow =
                GetHorizontalBoundaryRow(
                    0,
                    targetDoor);

            int targetWorldCol =
                sourceRoom.WorldCellCol +
                sourceDoor.CellCol -
                targetDoor.CellCol;

            return (
                targetWorldCol,
                sourceBoundaryRow -
                    targetBoundaryRow);
        }

        // Проверяет совпадение мировых границ и диапазонов двух дверей
        public static bool AreDoorsAligned(
            RoomInstance firstRoom,
            DoorSlot firstDoor,
            RoomInstance secondRoom,
            DoorSlot secondDoor)
        {
            if (firstDoor.Direction.Opposite() !=
                secondDoor.Direction)
            {
                return false;
            }

            if (firstDoor.Direction is
                Direction.Left or
                Direction.Right)
            {
                int firstBoundary =
                    GetVerticalBoundaryCol(
                        firstRoom.WorldCellCol,
                        firstDoor);

                int secondBoundary =
                    GetVerticalBoundaryCol(
                        secondRoom.WorldCellCol,
                        secondDoor);

                int firstRangeRow =
                    firstRoom.WorldCellRow +
                    firstDoor.CellRow;

                int secondRangeRow =
                    secondRoom.WorldCellRow +
                    secondDoor.CellRow;

                return firstBoundary ==
                           secondBoundary &&
                       firstRangeRow ==
                           secondRangeRow;
            }

            int firstHorizontalBoundary =
                GetHorizontalBoundaryRow(
                    firstRoom.WorldCellRow,
                    firstDoor);

            int secondHorizontalBoundary =
                GetHorizontalBoundaryRow(
                    secondRoom.WorldCellRow,
                    secondDoor);

            int firstRangeCol =
                firstRoom.WorldCellCol +
                firstDoor.CellCol;

            int secondRangeCol =
                secondRoom.WorldCellCol +
                secondDoor.CellCol;

            return firstHorizontalBoundary ==
                       secondHorizontalBoundary &&
                   firstRangeCol ==
                       secondRangeCol;
        }

        // Проверяет, что дверь действительно принадлежит указанному шаблону
        private static void ValidateDoorBelongsToTemplate(
            RoomTemplate template,
            DoorSlot door)
        {
            var templateDoor =
                template.GetDoor(door.Id);

            if (templateDoor is null ||
                templateDoor.Direction !=
                    door.Direction ||
                templateDoor.CellCol !=
                    door.CellCol ||
                templateDoor.CellRow !=
                    door.CellRow)
            {
                throw new InvalidOperationException(
                    $"Дверь {door.Id} не принадлежит шаблону {template.Id}.");
            }
        }

        // Проверяет встречное направление соединяемых дверей
        private static void ValidateOppositeDirections(
            DoorSlot sourceDoor,
            DoorSlot targetDoor)
        {
            if (sourceDoor.Direction.Opposite() !=
                targetDoor.Direction)
            {
                throw new InvalidOperationException(
                    $"Двери {sourceDoor.Id} и {targetDoor.Id} " +
                    "не направлены навстречу друг другу.");
            }
        }

        // Возвращает глобальный столбец вертикальной границы двери
        private static int GetVerticalBoundaryCol(
            int roomWorldCol,
            DoorSlot door)
        {
            return door.Direction switch
            {
                Direction.Left =>
                    roomWorldCol +
                    door.CellCol,

                Direction.Right =>
                    roomWorldCol +
                    door.CellCol + 1,

                _ =>
                    throw new ArgumentException(
                        "Ожидалась левая или правая дверь.",
                        nameof(door))
            };
        }

        // Возвращает глобальную строку горизонтальной границы двери
        private static int GetHorizontalBoundaryRow(
            int roomWorldRow,
            DoorSlot door)
        {
            return door.Direction switch
            {
                Direction.Top =>
                    roomWorldRow +
                    door.CellRow,

                Direction.Bottom =>
                    roomWorldRow +
                    door.CellRow + 1,

                _ =>
                    throw new ArgumentException(
                        "Ожидалась верхняя или нижняя дверь.",
                        nameof(door))
            };
        }
    }
}
