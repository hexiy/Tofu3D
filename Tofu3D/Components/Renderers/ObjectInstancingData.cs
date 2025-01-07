namespace Tofu3D.Rendering.Instancing;

/// <summary>
/// Every renderer/object needs ObjectInstancingData
/// </summary>
public struct ObjectInstancingData()
{
    [XmlIgnore]
    public int StartingIndexInBuffer = -1;

    [XmlIgnore]
    public int InstancedRenderingDefinitionIndex = -1;

    [XmlIgnore]
    internal bool InstancingDataDirty = true;

    [XmlIgnore]
    internal bool MatrixDirty = true;
}