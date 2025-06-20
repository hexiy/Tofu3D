using System.Linq;
using TofuEngine.Rendering;

namespace TofuEngine;

public class RenderingSystem
{
    private List<RenderTargetPipeline> _renderTargetPipelines = new List<RenderTargetPipeline>();
    public RenderTargetPipeline? CurrentlyExecutingPipeline;

    public RenderTargetPipeline GetGameViewPipeline() =>
        _renderTargetPipelines.First(pipeline => pipeline.ViewType is RenderTargetPipelineType.GameView);

    public void Initialize()
    {
        _renderTargetPipelines = new List<RenderTargetPipeline>();
    }

    public RenderTargetPipeline CreatePipelineForView(EditorPanelGenericView view, RenderTargetPipelineType type,
        int id, Vector2? viewSize = null)
    {
        RenderTargetPipeline pipeline = new RenderTargetPipeline(view, type);
        pipeline.Initialize(id, viewSize);


        _renderTargetPipelines.Add(pipeline);

        return pipeline;
    }

    public void DestroyPipeline(ref RenderTargetPipeline pipeline)
    {
        _renderTargetPipelines.Remove(pipeline);
        pipeline.Camera.GameObject.Destroy();

        pipeline = null;
    }

    public void RenderAllRenderTargetPipelines()
    {
        Tofu.SceneManager.CurrentScene.UploadRenderData( /*InstancingRenderMode.All*/);

        foreach (RenderTargetPipeline pipeline in _renderTargetPipelines)
        {
            CurrentlyExecutingPipeline = pipeline;
            // Tofu.SceneManager.CurrentScene.UploadRenderData(InstancingRenderMode.All);

            pipeline.RenderAllPasses();
        }

        // CurrentlyExecutingPipeline = null;
    }
}