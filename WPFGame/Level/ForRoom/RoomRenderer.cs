using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WPFGame.Core;

namespace WPFGame.Level
{
    // Преобразует данные комнаты в WPF-элементы на Canvas
    public static class RoomRenderer
    {
        // Отрисовывает все тайлы комнаты и возвращает созданные элементы
        public static List<Rectangle> Render(
            Canvas canvas,
            RoomTemplate room,
            double originX = 0,
            double originY = 0)
        {
            ArgumentNullException.ThrowIfNull(
                canvas);

            ArgumentNullException.ThrowIfNull(
                room);

            var renderedTiles =
                new List<Rectangle>();

            foreach (var tile in
                     room.Tiles)
            {
                Rectangle rectangle =
                    CreateTile(
                        tile,
                        originX,
                        originY);

                canvas.Children.Add(
                    rectangle);

                renderedTiles.Add(
                    rectangle);
            }

            return renderedTiles;
        }

        // Создаёт один WPF-тайл в мировых координатах комнаты
        private static Rectangle CreateTile(
            TileData tile,
            double originX,
            double originY)
        {
            var rectangle =
                new Rectangle
                {
                    Tag =
                        tile.Type.ToString(),

                    Width =
                        tile.Width,

                    Height =
                        tile.Height,

                    Fill =
                        GetBrush(
                            tile.Type),

                    Opacity =
                        GetOpacity(
                            tile.Type)
                };

            Panel.SetZIndex(
                rectangle,
                tile.Type is
                    TileType.SlopeUpRight or
                    TileType.SlopeUpLeft
                        ? ZLayer.Slopes
                        : ZLayer.Tiles);

            Canvas.SetLeft(
                rectangle,
                originX +
                tile.X);

            Canvas.SetTop(
                rectangle,
                originY +
                tile.Y);

            return rectangle;
        }

        // Возвращает цвет для типа тайла
        private static Brush GetBrush(
            TileType type)
        {
            return type switch
            {
                TileType.Ground =>
                    Brushes.DarkGray,

                TileType.Platform =>
                    Brushes.Aqua,

                TileType.Ladder =>
                    Brushes.SaddleBrown,

                TileType.SlopeUpRight =>
                    Brushes.LightSlateGray,

                TileType.SlopeUpLeft =>
                    Brushes.LightSlateGray,

                _ =>
                    Brushes.Magenta
            };
        }

        // Возвращает прозрачность для типа тайла
        private static double GetOpacity(
            TileType type)
        {
            return type switch
            {
                TileType.SlopeUpRight =>
                    0.8,

                TileType.SlopeUpLeft =>
                    0.8,

                _ =>
                    1.0
            };
        }
    }
}
