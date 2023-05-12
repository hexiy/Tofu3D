namespace Tofu3D;

public class InstancedRenderingObjectBufferData
{
	public float[] Buffer;
	public int NumberOfObjects;
	public int MaxNumberOfObjects;
	public int Vbo;
	public bool Dirty = true;
	public List<int> EmptyStartIndexes;
}