public static class Global
{
    public const string DebugFlag = nameof(Debug);
    public static bool GameRunning = false;
    public static bool EditorAttached = true;
    private static bool _debug;

    public static Action<bool> DebugStateChanged = b => { };

    public static bool Debug
    {
        get => _debug;
        set
        {
            _debug = value;
            SaveData();
            DebugStateChanged.Invoke(_debug);
        }
    }

    private static void SaveData()
    {
        PersistentData.Set("Global.Debug", Debug);
    }

    public static void LoadSavedData()
    {
        _debug = PersistentData.GetBool("Global.Debug", false);
    }
}