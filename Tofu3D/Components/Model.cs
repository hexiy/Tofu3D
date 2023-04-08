public class Model : Asset<Model>
{
	[XmlIgnore]
	public float[] VertexBufferData;
	// public float[] UVs;
	// public float[] Normals;
	// public uint[] Indices;

	public int Vao;
}