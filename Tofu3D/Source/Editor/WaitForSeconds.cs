namespace TofuEngine;

public class WaitForSeconds(float seconds) :YieldInstruction
{
    public float SecondsToWait { get; set; } = seconds;
}