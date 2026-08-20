namespace WPFGame.Level
{
    // GameTick сравнивает Tag через (string)element.Tag == "Ground" — TileType.Ground.ToString()
    public enum TileType
    {
        Ground,
        Platform,
        Ladder,
        SlopeUpRight,
        SlopeUpLeft
    }
}
