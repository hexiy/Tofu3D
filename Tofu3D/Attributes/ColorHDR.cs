namespace Tofu3D;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class ColorHDR : Attribute
{
    public float MinIntensity = -10;
    public float MaxIntensity =10;
}