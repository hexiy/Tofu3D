using System.Linq;
using System.Runtime.InteropServices;

public static class InstancedVertexDataLayoutDefinition
{
    // value, number of floats
    public static List<int> Members = new List<int>()
    {
        3, // Model1
        3, // Model2
        3, // Model3
        3, // Model4
        1, // MousePickingId
        4, // AlbedoTextureBoundsAndAtlasIndexPacked
    };

    private static int _countOfFloatsCached = -1;

    public static int CountOfFloats
    {
        get
        {
            if (_countOfFloatsCached == -1)
            {
                _countOfFloatsCached = Members.Sum();
            }

            return _countOfFloatsCached;
        }
    }

    private static int _totalSizeOfVertexInBytesCached = -1;

    public static int TotalSizeOfVertexInBytes
    {
        get
        {
            if (_totalSizeOfVertexInBytesCached == -1)
            {
                _totalSizeOfVertexInBytesCached = Members.Sum() * sizeof(float);
            }

            return _totalSizeOfVertexInBytesCached;
        }
    }
}