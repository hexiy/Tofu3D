namespace Tofu3D;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class Slider : Attribute
{
    public int MaxValue;
    public int MinValue;

    public Slider(int min, int max)
    {
        MinValue = min;
        MaxValue = max;
    }
}