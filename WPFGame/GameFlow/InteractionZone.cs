using System.Windows;

namespace WPFGame.GameFlow
{
    // Описывает область, в которой игрок может выполнить действие удержанием кнопки
    public sealed class InteractionZone
    {
        private bool completedDuringCurrentStay;
        private bool blockedUntilRelease;

        public Rect Bounds { get; }

        public string Prompt { get; }

        public double HoldDuration { get; }

        public double HoldProgress { get; private set; }

        public bool IsPlayerInside { get; private set; }

        public InteractionZone(
            Rect bounds,
            string prompt,
            double holdDuration)
        {
            if (bounds.Width <= 0 ||
                bounds.Height <= 0)
            {
                throw new ArgumentException(
                    "Зона взаимодействия должна иметь положительный размер.",
                    nameof(bounds));
            }

            if (string.IsNullOrWhiteSpace(
                    prompt))
            {
                throw new ArgumentException(
                    "Подсказка взаимодействия не должна быть пустой.",
                    nameof(prompt));
            }

            if (holdDuration <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(holdDuration),
                    "Время удержания должно быть положительным.");
            }

            Bounds =
                bounds;

            Prompt =
                prompt;

            HoldDuration =
                holdDuration;
        }

        // Обновляет нахождение игрока в зоне и возвращает true один раз после полного удержания
        public bool Update(
            Rect playerHitBox,
            bool isInteracting,
            double deltaSeconds)
        {
            if (deltaSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(deltaSeconds));
            }

            IsPlayerInside =
                Bounds.IntersectsWith(
                    playerHitBox);

            if (!IsPlayerInside)
            {
                ResetCurrentStay();

                return false;
            }

            if (blockedUntilRelease)
            {
                if (!isInteracting)
                {
                    blockedUntilRelease =
                        false;
                }

                HoldProgress =
                    0;

                return false;
            }

            if (completedDuringCurrentStay)
            {
                return false;
            }

            if (!isInteracting)
            {
                HoldProgress =
                    0;

                return false;
            }

            HoldProgress =
                Math.Min(
                    HoldDuration,
                    HoldProgress +
                    deltaSeconds);

            if (HoldProgress <
                HoldDuration)
            {
                return false;
            }

            completedDuringCurrentStay =
                true;

            return true;
        }

        // Возвращает прогресс удержания в диапазоне от 0 до 1
        public double GetProgressRatio()
        {
            return Math.Clamp(
                HoldProgress /
                HoldDuration,
                0,
                1);
        }

        // Требует отпустить кнопку перед следующим удержанием
        public void BlockUntilRelease()
        {
            blockedUntilRelease =
                true;

            HoldProgress =
                0;
        }

        // Полностью сбрасывает состояние зоны
        public void Reset()
        {
            IsPlayerInside =
                false;

            blockedUntilRelease =
                false;

            ResetCurrentStay();
        }

        private void ResetCurrentStay()
        {
            HoldProgress =
                0;

            completedDuringCurrentStay =
                false;
        }
    }
}
