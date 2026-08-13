namespace WPFGame.GameFlow
{
    // Хранит состояние игрового цикла между отдельными сценами
    public sealed class GameSession
    {
        public GameSceneType CurrentScene { get; private set; }

        public int StationNumber { get; private set; }

        public GameSession()
        {
            CurrentScene =
                GameSceneType.Train;

            StationNumber =
                0;
        }

        // Переводит игровой цикл в состояние поезда
        public void EnterTrain()
        {
            CurrentScene =
                GameSceneType.Train;
        }

        // Отмечает начало следующей станции
        public void StartStation()
        {
            StationNumber++;

            CurrentScene =
                GameSceneType.Station;
        }
    }
}
