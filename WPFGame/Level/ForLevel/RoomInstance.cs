namespace WPFGame.Level
{
    // Описывает конкретное размещение шаблона комнаты в уровне
    public class RoomInstance
    {
        public string Id { get; }
        public RoomTemplate Template { get; }

        public int WorldCellCol { get; }
        public int WorldCellRow { get; }

        public double OriginX =>
            WorldCellCol *
            RoomMetrics.CellWidth;

        public double OriginY =>
            WorldCellRow *
            RoomMetrics.CellHeight;

        public double Width =>
            Template.Width;

        public double Height =>
            Template.Height;

        public RoomInstance(
            string id,
            RoomTemplate template,
            int worldCellCol,
            int worldCellRow)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "ID экземпляра комнаты не должен быть пустым.",
                    nameof(id));
            }

            Id = id;
            Template = template ??
                throw new ArgumentNullException(
                    nameof(template));

            WorldCellCol = worldCellCol;
            WorldCellRow = worldCellRow;
        }

        // Возвращает дверь шаблона или сообщает об ошибке структуры уровня
        public DoorSlot GetRequiredDoor(
            string doorId)
        {
            var door =
                Template.GetDoor(doorId);

            if (door is null)
            {
                throw new InvalidOperationException(
                    $"Дверь {doorId} не найдена в экземпляре {Id} " +
                    $"шаблона {Template.Id}.");
            }

            return door;
        }

        // Возвращает глобальные координаты занятых блоков комнаты
        public IEnumerable<(int Col, int Row)>
            GetOccupiedWorldCells()
        {
            foreach (var cell in
                     Template.OccupiedCells)
            {
                yield return (
                    WorldCellCol + cell.Col,
                    WorldCellRow + cell.Row);
            }
        }
    }
}
