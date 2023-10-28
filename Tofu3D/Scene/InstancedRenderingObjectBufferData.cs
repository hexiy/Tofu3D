namespace Tofu3D;

public class InstancedRenderingObjectBufferData
{
    public float[] Buffer;
    public int NumberOfObjects;
    public int MaxNumberOfObjects;
    public int FutureMaxNumberOfObjects;
    public int Vbo;

    public bool NeedsUpload = true;

    // public bool IsResizing = false;
    public List<int> EmptyStartIndexes;
}