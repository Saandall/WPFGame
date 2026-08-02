using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPFGame.Level
{
    // Единственное место, которое превращает данные (TileData) в визуальные объекты (Rectangle).
    // GameTick про этот класс ничего не знает и не должен — он просто видит готовые
    // прямоугольники на Canvas с нужным Tag, как и раньше при хардкоде в XAML.
    public static class RoomSpawner
    {
        // визуализирует комнату: (куда, какую комнату)
        public static void Spawn(Canvas canvas, RoomTemplate room)
        {
            foreach (var tile in room.Tiles)
            {
                canvas.Children.Add(CreateTile(tile));
            }
        }

        // Создаёт Rectangle под конкретный тайл, но НЕ добавляет его на Canvas —
        // это оставляем вызывающему коду (Spawn делает это сам, RoomManager — по-своему,
        // чтобы держать список созданных тайлов и уметь их убрать при смене комнаты).
        public static Rectangle CreateTile(TileData tile)
        {
            var rect = new Rectangle
            {
                Tag = tile.Type.ToString(), // "Ground", "Platform" и т.д. — как раньше в XAML
                Width = tile.Width,
                Height = tile.Height,
                Fill = GetBrush(tile.Type),
                Opacity = GetOpacity(tile.Type)
            };

            // Zindex, чтобы тайлы всегда оказывались под игроком (кроме склонов —
            // они и раньше в XAML были нарисованы поверх игрока, полупрозрачные).
            Panel.SetZIndex(rect, tile.Type is TileType.SlopeUpRight or TileType.SlopeUpLeft ? 20 : 0);

            Canvas.SetLeft(rect, tile.X);
            Canvas.SetTop(rect, tile.Y);

            return rect;
        }

        private static Brush GetBrush(TileType type) => type switch
        {
            TileType.Ground => Brushes.DarkGray,
            TileType.Platform => Brushes.Aqua,
            TileType.Ladder => Brushes.SaddleBrown,
            TileType.SlopeUpRight => Brushes.LightSlateGray,
            TileType.SlopeUpLeft => Brushes.LightSlateGray,
            _ => Brushes.Magenta // если забудете замапить новый тип — увидите яркую заглушку, а не тишину
        };

        private static double GetOpacity(TileType type) => type switch
        {
            TileType.SlopeUpRight => 0.8,
            TileType.SlopeUpLeft => 0.8,
            _ => 1.0
        };
    }
}
