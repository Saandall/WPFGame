using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPFGame.Level
{
    // Превращает данные тайлов в WPF-объекты
    public static class RoomSpawner
    {
        public static void Spawn(
            Canvas canvas,
            RoomTemplate room,
            double originX = 0,
            double originY = 0)
        {
            foreach (var tile in room.Tiles)
            {
                canvas.Children.Add(
                    CreateTile(
                        tile,
                        originX,
                        originY));
            }
        }

        // Создаёт тайл в мировых координатах комнаты
        public static Rectangle CreateTile(
            TileData tile,
            double originX = 0,
            double originY = 0)
        {
            var rectangle = new Rectangle
            {
                Tag = tile.Type.ToString(),
                Width = tile.Width,
                Height = tile.Height,
                Fill = GetBrush(tile.Type),
                Opacity = GetOpacity(tile.Type)
            };

            Panel.SetZIndex(
                rectangle,
                tile.Type is
                    TileType.SlopeUpRight or
                    TileType.SlopeUpLeft
                        ? 20
                        : 0);

            Canvas.SetLeft(
                rectangle,
                originX + tile.X);

            Canvas.SetTop(
                rectangle,
                originY + tile.Y);

            return rectangle;
        }

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

        private static double GetOpacity(
            TileType type)
        {
            return type switch
            {
                TileType.SlopeUpRight => 0.8,
                TileType.SlopeUpLeft => 0.8,
                _ => 1.0
            };
        }
    }
}
