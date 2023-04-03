[Serializable]
public class Model : Asset<Model>
{
	public float[] Vertices =
	{
		-0.5f, -0.5f, 0, 0,
		0.5f, -0.5f, 1, 0,
		-0.5f, 0.5f, 0, 1,
		-0.5f, 0.5f, 0, 1,
		0.5f, -0.5f, 1, 0,
		0.5f, 0.5f, 1, 1
	};
}