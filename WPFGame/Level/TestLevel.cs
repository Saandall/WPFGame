namespace WPFGame.Level
{
    // Сохраняет старое имя точки входа на время перехода к LevelLayout
    public static class TestLevel
    {
        public static LevelLayout Create()
        {
            return FixedLevelFactory.Create();
        }
    }
}
