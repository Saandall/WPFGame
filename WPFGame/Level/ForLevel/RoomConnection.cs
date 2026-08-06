namespace WPFGame.Level
{
    // Указывает конкретную дверь конкретного экземпляра комнаты
    public readonly record struct RoomDoorReference(
        string RoomInstanceId,
        string DoorId);

    // Хранит двустороннюю связь между двумя дверями уровня
    public class RoomConnection
    {
        public RoomDoorReference First { get; }
        public RoomDoorReference Second { get; }

        public RoomConnection(
            RoomDoorReference first,
            RoomDoorReference second)
        {
            if (first == second)
            {
                throw new ArgumentException(
                    "Дверь нельзя соединить саму с собой.");
            }

            First = first;
            Second = second;
        }

        // Проверяет принадлежность двери этой связи
        public bool Contains(
            RoomDoorReference endpoint)
        {
            return endpoint == First ||
                   endpoint == Second;
        }

        // Возвращает дверь на противоположной стороне связи
        public RoomDoorReference GetOther(
            RoomDoorReference endpoint)
        {
            if (endpoint == First)
            {
                return Second;
            }

            if (endpoint == Second)
            {
                return First;
            }

            throw new InvalidOperationException(
                "Указанная дверь не принадлежит этой связи.");
        }
    }
}
