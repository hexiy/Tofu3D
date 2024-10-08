using System.Threading;

public class StressTester : Component, IComponentUpdateable
{
    [Show] public int Milliseconds = 1;

    public void Update()
    {
        Milliseconds = Mathf.Clamp(Milliseconds, 0, int.MaxValue);
        Thread.Sleep(Milliseconds);
    }
}