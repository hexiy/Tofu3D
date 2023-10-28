namespace Scripts;

public struct RendererInstancingData
{
    [XmlIgnore]
    public int InstancedRenderingStartingIndexInBuffer { get; set; } = -1;

    [XmlIgnore]
    public int InstancedRenderingDefinitionIndex = -1;

    [XmlIgnore]
    internal bool InstancingDataDirty { get; set; } = true;

    [XmlIgnore]
    internal bool MatrixDirty { get; set; } = true;

    public RendererInstancingData()
    {
        InstancedRenderingStartingIndexInBuffer = -1;
        InstancedRenderingDefinitionIndex = -1;
        InstancingDataDirty = true;
    }
}