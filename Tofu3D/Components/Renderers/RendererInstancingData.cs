namespace Scripts;

public struct RendererInstancingData
{
    [XmlIgnore] public int InstancedRenderingStartingIndexInBuffer = -1;

    [XmlIgnore] public int InstancedRenderingDefinitionIndex = -1;

    [XmlIgnore] internal bool InstancingDataDirty = true;

    [XmlIgnore] internal bool MatrixDirty = true;

    public RendererInstancingData()
    {
        InstancedRenderingStartingIndexInBuffer = -1;
        InstancedRenderingDefinitionIndex = -1;
        InstancingDataDirty = true;
    }
}