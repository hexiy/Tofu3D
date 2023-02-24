using System.Threading;

public class StressTester : Component
{
	[Show]
	public int Milliseconds = 1;

	public override void Update()
	{
		Milliseconds = Mathf.Clamp(Milliseconds, 0, int.MaxValue);
		Thread.Sleep(Milliseconds);
		base.Update();
	}
}