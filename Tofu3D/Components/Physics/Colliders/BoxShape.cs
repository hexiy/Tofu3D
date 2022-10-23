namespace Scripts;

public class BoxShape : Shape
{
	public Vector3 Offset = Vector3.Zero;
	public Vector3 Size;

	public Vector3 GetMinPos()
	{
		return Transform.WorldPosition;
	}

	public Vector3 GetMaxPos()
	{
		return Transform.WorldPosition + Size;
	}
}