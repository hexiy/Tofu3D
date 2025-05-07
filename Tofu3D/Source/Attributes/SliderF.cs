namespace TofuEngine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SliderF : Attribute
{
    public float MaxValue;
    public float MinValue;
    public bool AllowCustomValue;

    public SliderF(float min, float max, bool allowCustomValue = false)
    {
        MinValue = min;
        MaxValue = max;
        AllowCustomValue = allowCustomValue;
    }
}