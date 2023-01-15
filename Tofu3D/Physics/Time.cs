namespace Tofu3D;

public static class Time
{
	public static float DeltaTime = 0.01666666f;
	public static float EditorDeltaTime = 0.01666666f;
	public static float FixedDeltaTime = 0.01f;
	public static float ElapsedTime;
	public static float EditorElapsedTime;
	public static float EditorElapsedTicks;
	public static float ElapsedSeconds;
	public static ulong ElapsedTicks;
	public static ulong TimeScale = 0;

	public static void Update()
	{
		EditorDeltaTime = (float) Window.I.UpdateTime;
		EditorElapsedTime += EditorDeltaTime;
		EditorElapsedTicks++;

		if (Global.GameRunning)
		{
			DeltaTime = (float) Window.I.UpdateTime;

			ElapsedTime += DeltaTime;
			ElapsedSeconds = ElapsedTime;
			ElapsedTicks++;
		}
		else
		{
			DeltaTime = 0;
		}
	}
}