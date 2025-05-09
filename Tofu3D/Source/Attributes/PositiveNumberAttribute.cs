namespace TofuEngine;
[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class PositiveNumberAttribute : NumberRangeLimiterAttribute
{
    public PositiveNumberAttribute() : base(min: 0, max: null)
    {
    }
}