namespace TofuEngine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SliderFAttribute : Attribute
{
    public float MaxValue;
    public float MinValue;
    public bool AllowCustomValue;

    public SliderFAttribute(float min, float max, bool allowCustomValue = false)
    {
        MinValue = min;
        MaxValue = max;
        AllowCustomValue = allowCustomValue;
    }
}