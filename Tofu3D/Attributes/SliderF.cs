namespace Tofu3D;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SliderF : Attribute
{
	public float MaxValue;
	public float MinValue;

	public SliderF(float min, float max)
	{
		MinValue = min;
		MaxValue = max;
	}
}