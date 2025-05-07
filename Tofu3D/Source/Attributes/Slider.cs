namespace TofuEngine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class Slider : Attribute
{
    public int MaxValue;
    public int MinValue;
    public bool AllowCustomValue;

    public Slider(int min, int max, bool allowCustomValue = false)
    {
        MinValue = min;
        MaxValue = max;
        AllowCustomValue = allowCustomValue;
    }
}