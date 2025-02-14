namespace TofuEngine;

public interface IComponentRenderable : IComparable<IComponentRenderable>
{
    public void UploadRenderData();
    public RenderMode RenderMode { get; }
    public int RenderOrder { get; set; }
}