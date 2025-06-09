namespace TofuEngine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NumberRangeLimiterAttribute : Attribute
{
    public int? MaxValueInt = null;
    public int? MinValueInt = null;

    public float? MaxValueFloat = null;
    public float? MinValueFloat = null;

    public NumberRangeLimiterAttribute(float? min, float? max)
    {
        MinValueFloat = min;
        MaxValueFloat = max;
    }

    public NumberRangeLimiterAttribute(int? min, int? max)
    {
        MinValueInt = min;
        MaxValueInt = max;
    }
    
    public NumberRangeLimiterAttribute(int min, int max)
    {
        MinValueInt = min;
        MaxValueInt = max;
    }
}