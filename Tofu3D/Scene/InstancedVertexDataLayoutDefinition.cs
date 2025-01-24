using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InstancedVertexDataLayoutDefinition
{
    public Vector3 Model1;
    public Vector3 Model2;
    public Vector3 Model3;
    public Vector3 Model4;
    public float MousePickingId;
    public Vector4 AlbedoTextureBoundsAndAtlasIndexPacked;
    public Vector4 NormalTextureBoundsAndAtlasIndexPacked;
    // bounds 0-1 = atlas 1, 1-2 will be atlas 2, 2-3 will be atlas 3 etc.....

    // Pre-calculate the total size in bytes.
    public static readonly int SizeInBytes = Marshal.SizeOf<InstancedVertexDataLayoutDefinition>();

    // Pre-calculate field offsets to avoid reflection.
    public static readonly int Model1Offset = 0;
    public static readonly int Model2Offset = Model1Offset + sizeof(float) * 3;
    public static readonly int Model3Offset = Model2Offset + sizeof(float) * 3;
    public static readonly int Model4Offset = Model3Offset + sizeof(float) * 3;
    public static readonly int UVOffsetOffset = Model4Offset + sizeof(float) * 3;
    public static readonly int MousePickingIdOffset = UVOffsetOffset + sizeof(float) * 2;
    public static readonly int AlbedoTextureBoundsOffset = MousePickingIdOffset + sizeof(float);
}