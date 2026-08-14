using System;

namespace WPFGame.GameFlow
{
    // Хранит состояние игрового цикла между отдельными сценами
    public sealed class GameSession
    {
        public GameSceneType CurrentScene { get; private set; }

        public int StationNumber { get; private set; }

        public int? CurrentSeed { get; private set; }

        public GameSession()
        {
            CurrentScene =
                GameSceneType.Train;

            StationNumber =
                0;

            CurrentSeed =
                null;
        }

        // Переводит игровой цикл в состояние поезда
        public void EnterTrain()
        {
            CurrentScene =
                GameSceneType.Train;

            CurrentSeed =
                null;
        }

        // Начинает следующую станцию и возвращает её seed
        public int StartStation(
            int? seed = null)
        {
            StationNumber++;

            CurrentSeed =
                seed ??
                Random.Shared.Next();

            CurrentScene =
                GameSceneType.Station;

            return CurrentSeed.Value;
        }
    }
}
