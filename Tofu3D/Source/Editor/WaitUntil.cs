namespace TofuEngine;

public class WaitUntil(Func<bool> waitCondition) :YieldInstruction
{
    public Func<bool> WaitCondition { get; init; } = waitCondition;
}