namespace TofuEngine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SliderAttribute : Attribute
{
    public int MaxValue;
    public int MinValue;
    public bool AllowCustomValue;

    public SliderAttribute(int min, int max, bool allowCustomValue = false)
    {
        MinValue = min;
        MaxValue = max;
        AllowCustomValue = allowCustomValue;
    }
}