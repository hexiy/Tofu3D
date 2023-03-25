public static class Global
{
	public static bool GameRunning = false;
	public static bool EditorAttached = true;
	static bool _debug;
	public static bool Debug
	{
		get { return _debug; }
		set
		{
			_debug = value;
			SaveData();
		}
	}
	public const string DebugFlag = nameof(Debug);

	static void SaveData()
	{
		PersistentData.Set("Global.Debug", Debug);
	}

	public static void LoadSavedData()
	{
		_debug = PersistentData.GetBool("Global.Debug", false);
	}
}