namespace Tofu3D;

public interface IComponentRenderable// : IComparable<IComponentRenderable>
{
    public void Render();
    public RenderMode RenderMode { get; }
    // public int RenderOrder { get; }
}