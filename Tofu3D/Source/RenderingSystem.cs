using TofuEngine.Rendering;

namespace Tofu3D;

public class RenderingSystem
{
    private List<RenderTargetPipeline> _renderTargetPipelines = new List<RenderTargetPipeline>();
    public RenderTargetPipeline? CurrentlyExecutingPipeline;

    public void Initialize()
    {
        _renderTargetPipelines = new List<RenderTargetPipeline>();
    }

    public RenderTargetPipeline CreatePipeline()
    {
        RenderTargetPipeline pipeline = new RenderTargetPipeline();
        pipeline.Initialize();


        _renderTargetPipelines.Add(pipeline);

        return pipeline;
    }

    public void RenderAllRenderTargetPipelines()
    {
        foreach (RenderTargetPipeline pipeline in _renderTargetPipelines)
        {
            CurrentlyExecutingPipeline = pipeline;
            pipeline.RenderAllPasses();
        }

        CurrentlyExecutingPipeline = null;
    }
}