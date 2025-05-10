using TofuEngine.Rendering;

namespace Tofu3D;

public class RenderingSystem
{
    private List<RenderTargetPipeline> _renderTargetPipelines = new List<RenderTargetPipeline>();
    public RenderTargetPipeline SceneViewPipeline;

    public void Initialize()
    {
        _renderTargetPipelines = new List<RenderTargetPipeline>();


        SceneViewPipeline = new RenderTargetPipeline();
        SceneViewPipeline.Initialize();


        _renderTargetPipelines.Add(SceneViewPipeline);
    }

    public void RenderAllRenderTargetPipelines()
    {
        foreach (RenderTargetPipeline pipeline in _renderTargetPipelines)
        {
            pipeline.RenderAllPasses();
        }
    }
}