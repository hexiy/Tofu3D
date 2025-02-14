namespace TofuEngine;

public class WaitWhile(Func<bool> waitCondition) :YieldInstruction
{
    public Func<bool> WaitCondition { get; init; } = waitCondition;
}