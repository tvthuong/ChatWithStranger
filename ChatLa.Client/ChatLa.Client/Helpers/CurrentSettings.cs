namespace ChatLa.Client.Helpers
{
    public static class CurrentSettings
    {
        public static UserSettings Settings { get; set; }
        static CurrentSettings()
        {
            Settings = new UserSettings();
        }
    }
}
