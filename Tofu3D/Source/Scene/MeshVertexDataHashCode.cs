using Microsoft.DotNet.PlatformAbstractions;

public struct MeshVertexDataHashCode(Mesh mesh, int id)
{
    public int DataHashCode => GetHashCode();

    private int GetHashCode()
    {
        HashCodeCombiner hashCodeCombiner = HashCodeCombiner.Start();

        hashCodeCombiner.Add(mesh.GeometryBufferData.GetHashCode());
        hashCodeCombiner.Add(id.GetHashCode());
        return hashCodeCombiner.CombinedHash;
    }
}