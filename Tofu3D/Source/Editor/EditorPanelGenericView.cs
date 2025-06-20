using TofuEngine.Rendering;

namespace TofuEngine;

public class EditorPanelGenericView : EditorPanel
{
    protected RenderTargetPipeline? _renderTargetPipeline;
    public RenderTargetPipelineType? ViewType => _renderTargetPipeline?.ViewType;

    public Camera? Camera => _renderTargetPipeline?.Camera;

    // public Vector2 MousePositionInView;
    public Vector2 ActualViewSize;
    public Vector2 ActualViewPosition;
    public float Scale => (float)ActualViewSize.X / _renderTargetPipeline.FramebufferSize.X;


    public override void Init()
    {
        if (EditorViewManager.LastHoveredView == null)
        {
            EditorViewManager.LastHoveredView = this;
        }

        if (EditorViewManager.LastUsedView == null)
        {
            EditorViewManager.LastUsedView = this;
        }

        base.Init();
    }

    protected override void ExecuteImGuiDrawCommands()
    {
    }
}