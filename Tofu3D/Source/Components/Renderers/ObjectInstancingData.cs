namespace TofuEngine.Rendering.Instancing;

/// <summary>
/// Every renderer/object needs ObjectInstancingData
/// </summary>
public class ObjectInstancingData()
{
    [XmlIgnore]
    public Guid Guid = Guid.NewGuid();
    
    [XmlIgnore]
    public int StartingIndexInBuffer = -1;

    [XmlIgnore]
    public int InstancedRenderingDefinitionIndex = -1;

    [XmlIgnore]
    internal bool InstancingDataDirty = true;

    [XmlIgnore]
    internal bool MatrixDirty = true;
}