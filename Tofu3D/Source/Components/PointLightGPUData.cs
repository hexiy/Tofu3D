using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct PointLightGPUData
{
    public Vector3 Position; // 12 bytes
    public float Intensity; // 16 bytes
    public Vector3 Color; // 12 bytes
    public float Radius; // 16 bytes
}