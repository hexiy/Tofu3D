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

    public RenderTargetPipeline CreatePipeline(RenderTargetPipelineType type)
    {
        RenderTargetPipeline pipeline = new RenderTargetPipeline(type);
        pipeline.Initialize();


        _renderTargetPipelines.Add(pipeline);

        return pipeline;
    }

    public void RenderAllRenderTargetPipelines()
    {
        foreach (RenderTargetPipeline pipeline in _renderTargetPipelines)
        {
            CurrentlyExecutingPipeline = pipeline;
            Tofu.SceneManager.CurrentScene.UploadRenderData(InstancingRenderMode.All);

            pipeline.RenderAllPasses();
        }

        // CurrentlyExecutingPipeline = null;
    }
}