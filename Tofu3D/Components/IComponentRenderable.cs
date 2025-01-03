namespace Tofu3D;

public interface IComponentRenderable
{
    public void Render();
    public RenderMode RenderMode { get; }
}