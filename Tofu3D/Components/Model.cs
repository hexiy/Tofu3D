[Serializable]
public class Model : Asset<Model>
{
	public float[] Vertices;
	public float[] UVs;
	public float[] Normals;

	public int Vao;
}