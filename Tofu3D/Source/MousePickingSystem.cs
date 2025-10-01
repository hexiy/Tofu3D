using System.Linq;
using TofuEngine.Rendering;

namespace TofuEngine;

public static class MousePickingSystem
{
    static Dictionary<uint, Renderer> _renderers = new Dictionary<uint, Renderer>();

    static uint _lastPixel;

    private static uint _currentId;

    public static Renderer HoveredRenderer { get; private set; }

    public static uint RegisterObject(Renderer renderer)
    {
        // MousePickingObject mousePickingObject = new MousePickingObject() {Renderer = renderer, Color = GetFreeColor()};
        uint col = GetFreeColor();
        // Debug.Log($"registered mouse picking object with color {col}:rgba:{new Color(col)}, {col}");
        _renderers[col] = renderer;
        // _renderers.Add(mousePickingObject);
        return col;
    }

    public static void RemoveObject(Renderer renderer)
    {
        _renderers.Remove(renderer.MousePickingId);
    }

    // public static uint GetColor(Renderer renderer)
    // {
    // 	string timerName = $"PickingSystem GetColor w {_renderers.Count} renderers";
    // 	Debug.StartTimer(timerName);
    // 	foreach (MousePickingObject mousePickingObject in _renderers)
    // 	{
    // 		if (mousePickingObject.RendererId == renderer)
    // 		{
    // 			Debug.EndAndLogTimer(timerName);
    // 			return mousePickingObject.Color;
    // 		}
    // 	}
    //
    // 	Debug.EndAndLogTimer(timerName);
    // 	return 0;
    // }

    private static Renderer GetRenderer(uint color)
    {
        return _renderers.GetValueOrDefault(color, null);
        // ref Renderer valOrNew = ref CollectionsMarshal.GetValueRefOrNullRef(_renderers, color);
        //
        // return valOrNew;

        // return _renderers[color];
        // foreach (MousePickingObject mousePickingObject in _renderers)
        // {
        // 	if (mousePickingObject.Color == color)
        // 	{
        // 		return mousePickingObject.Renderer;
        // 	}
        // }

        // return null;
    }

    private static uint GetFreeColor()
    {
        return (uint)_renderers.Count + 1;
        /*byte r = (byte)Random.Range(0, 256);
        byte g = (byte)Random.Range(0, 256);
        byte b = (byte)Random.Range(0, 256);
        byte a = 255; // Ensure alpha is always 255 (fully opaque)

        // Pack the RGBA values into a single uint
        return (uint)(a << 24 | r << 16 | g << 8 | b);*/

        // int r = (int) Mathf.ClampMax(_renderers.Count, 255);
        // int g = (int) Mathf.ClampMax(_renderers.Count % 255 - r, 255);
        // int b = (int) Mathf.ClampMax((_renderers.Count % 255) % 255 - r - g, 255);
        // return new Color(r, g, b, 0);
    }

    public static void Initialize()
    {
        //_renderers = new HashSet<MousePickingObject>();

        EditorPanelTextureViewer.AddTexture(new TextureViewerTextureData()
        {
            Name = "Mouse Picking",
            Texture = Tofu.RenderingSystem.GetSceneViewPipeline().RenderPasses
                .First(pass => pass.RenderPassType is RenderPassType.MousePicking).MainFramebuffer
        });
    }

    // static void RenderPassMousePicking()
    // {
    //     // Tofu.SceneManager.CurrentScene.RenderAll();
    //     Tofu.SceneManager.CurrentScene.RenderOpaques();
    //     Tofu.SceneManager.CurrentScene.RenderTransparency();
    // }

    public static unsafe void ReadPixelAtMousePos(RenderPass renderPass)
    {
        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, renderPass.MainFramebuffer.FrameBufferID);
        // GL.Viewport(0, 0, RenderPassMousePicking.I.MainFramebuffer.Size.Xi,
            // RenderPassMousePicking.I.MainFramebuffer.Size.Yi);

        // GL.ReadPixels(Tofu.MouseInput.PositionInHoveredView.Xi, Tofu.MouseInput.PositionInHoveredView.Yi, 1, 1,
            // PixelFormat.Rgba,
            // PixelType.UnsignedByte, ref _currentPixel);

        if (_renderers.Count == 0)
        {
            return;
        }

        byte[] pixel = new byte[4];
        GL.ReadPixels((int)Tofu.MouseInput.PositionInHoveredView.X,
            (int)Tofu.MouseInput.PositionInHoveredView.Y, 1, 1,
            PixelFormat.Rgba, PixelType.UnsignedByte, pixel);

        byte r = pixel[0];
        byte g = pixel[1];
        byte b = pixel[2];
        byte a = pixel[3];

        _currentId = (uint)((a << 24) | (r << 16) | (g << 8) | b);

        if (_currentId != 0)
        {
            // Debug.Log($"Mouse picking hit id={_currentId} rgba=({r},{g},{b},{a})");
        }

        // GL.Viewport();
        // GL.ReadPixels(idk, idk, 1, 1,
        // PixelFormat.Rgba, PixelType.UnsignedByte, ref _tempPixels);

        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
    }

    // find renderer in Update, so we're not slowing down rendering/inflating the numbers
    public static void Update()
    {
        // if (Tofu.MouseInput.IsMouseInSceneView == false || Tofu.MouseInput.IsButtonDown() == false)
        // {
        // return;
        // }

        if (_currentId != _lastPixel)
        {
            _lastPixel = _currentId;
            HoveredRenderer = GetRenderer(_currentId); // find renderer by decoded id

            byte a = (byte)((_currentId >> 24) & 0xFF);
            byte r = (byte)((_currentId >> 16) & 0xFF);
            byte g = (byte)((_currentId >> 8) & 0xFF);
            byte b = (byte)(_currentId & 0xFF);
            // Debug.Log($"Extracted ID bytes: R={r}, G={g}, B={b}, A={a}");

            if (HoveredRenderer != null)
            {
                // Debug.Log($"HoveredRenderer:{HoveredRenderer.GameObject.Name}");
            }
            else
            {
                // Debug.Log("Couldn't find any renderer hovered");
            }
        }

        Debug.StatSetValue("HoveredRenderer", $"HoveredRenderer {HoveredRenderer?.GameObject?.Name}");

        if (Tofu.MouseInput.ButtonPressed())
        {
            // Debug.Log($"selected:{HoveredRenderer?.GameObject.Name ?? "none"}");

            // dont detect clicks on the transformhandle/selection highlighter box
            if (HoveredRenderer?.GameObject.VisibleInHierarchy == true)
            {
                Tofu.Editor.AfterDraw += () =>
                    Tofu.GameObjectSelectionManager.SelectGameObject(HoveredRenderer.GameObject);
            }
            else if (HoveredRenderer == null)
                // else if(HoveredRenderer?.GameObjectId!=TransformHandle.I.GameObjectId) // if we're dragging transformhandle we dont want to deselect anything
            {
                // Tofu.GameObjectSelectionManager.Deselect();
            }
        }
        // Color color = new Color(_pixels);
        // Debug.StatSetValue("picking color", $"Picking hovered color:{color.ToString()}");
    }
}