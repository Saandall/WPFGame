using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPFGame.Core;

namespace WPFGame.GameFlow
{
    // Показывает HUD-подсказку и прогресс удержания для активной зоны взаимодействия
    public sealed class InteractionPrompt
    {
        private const double PanelWidth = 380;
        private const double PanelHeight = 72;

        private readonly Canvas viewport;
        private readonly Border panel;
        private readonly TextBlock promptText;
        private readonly ProgressBar progressBar;

        public InteractionPrompt(
            Canvas viewport)
        {
            this.viewport =
                viewport ??
                throw new ArgumentNullException(
                    nameof(viewport));

            promptText =
                new TextBlock
                {
                    Foreground =
                        Brushes.White,

                    FontSize =
                        17,

                    FontWeight =
                        FontWeights.SemiBold,

                    TextAlignment =
                        TextAlignment.Center,

                    HorizontalAlignment =
                        HorizontalAlignment.Stretch,

                    Margin =
                        new Thickness(
                            8,
                            6,
                            8,
                            4)
                };

            progressBar =
                new ProgressBar
                {
                    Minimum =
                        0,

                    Maximum =
                        1,

                    Height =
                        10,

                    Margin =
                        new Thickness(
                            14,
                            0,
                            14,
                            8),

                    IsHitTestVisible =
                        false
                };

            var content =
                new StackPanel();

            content.Children.Add(
                promptText);

            content.Children.Add(
                progressBar);

            panel =
                new Border
                {
                    Width =
                        PanelWidth,

                    Height =
                        PanelHeight,

                    Background =
                        new SolidColorBrush(
                            Color.FromArgb(
                                215,
                                15,
                                17,
                                21)),

                    BorderBrush =
                        Brushes.White,

                    BorderThickness =
                        new Thickness(
                            1),

                    CornerRadius =
                        new CornerRadius(
                            5),

                    Child =
                        content,

                    Visibility =
                        Visibility.Collapsed,

                    IsHitTestVisible =
                        false
                };

            Canvas.SetLeft(
                panel,
                (viewport.Width -
                 PanelWidth) /
                2);

            Canvas.SetTop(
                panel,
                viewport.Height -
                PanelHeight -
                28);

            Panel.SetZIndex(
                panel,
                ZLayer.Interface + 20);

            viewport.Children.Add(
                panel);
        }

        // Показывает подсказку текущей зоны и её прогресс удержания
        public void Show(
            InteractionZone zone)
        {
            ArgumentNullException.ThrowIfNull(
                zone);

            promptText.Text =
                zone.Prompt;

            progressBar.Value =
                zone.GetProgressRatio();

            panel.Visibility =
                Visibility.Visible;
        }

        // Скрывает подсказку вне зоны взаимодействия
        public void Hide()
        {
            panel.Visibility =
                Visibility.Collapsed;

            progressBar.Value =
                0;
        }

        // Удаляет HUD-элемент из Viewport при смене игровой сцены
        public void Remove()
        {
            viewport.Children.Remove(
                panel);
        }
    }
}
