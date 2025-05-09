namespace TofuEngine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class ColorHDRAttribute : Attribute
{
    public float MinIntensity = -10;
    public float MaxIntensity =10;
}