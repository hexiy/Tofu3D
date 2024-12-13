using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tofu3D.Rendering;

namespace Tofu3D;

public static class MousePickingSystem
{
    static Dictionary<uint, Renderer> _renderers = new Dictionary<uint, Renderer>();

    static uint _lastPixel;

    // static uint _tempPixels;
    private static uint _currentPixel;

    public static Renderer HoveredRenderer { get; private set; }

    public static uint RegisterObject(Renderer renderer)
    {
        // MousePickingObject mousePickingObject = new MousePickingObject() {Renderer = renderer, Color = GetFreeColor()};
        uint col = GetFreeColor();
        Debug.Log($"registered mouse picking object with color {col}:rgba:{new Color(col)}");
        _renderers[col] = renderer;
        // _renderers.Add(mousePickingObject);
        return col;
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
        // int r = (int) Mathf.ClampMax(_renderers.Count, 255);
        // int g = (int) Mathf.ClampMax(_renderers.Count % 255 - r, 255);
        // int b = (int) Mathf.ClampMax((_renderers.Count % 255) % 255 - r - g, 255);
        // return new Color(r, g, b, 0);
    }

    public static void Initialize()
    {
        //_renderers = new HashSet<MousePickingObject>();
        Tofu.RenderPassSystem.RegisterRender(RenderPassType.MousePicking, RenderPassMousePicking);
        EditorPanelTextureViewer.AddTexture(new TextureViewerTextureData()
        {
            Name = "Mouse Picking",
            Texture = Tofu3D.Rendering.RenderPassMousePicking.I.MainFramebuffer
        });
    }

    static void RenderPassMousePicking()
    {
        Tofu.SceneManager.CurrentScene.RenderWorld();
    }

    public static unsafe void ReadPixelAtMousePos()
    {
        // GL.ReadPixels(0,0,1,1,PixelFormat.Rgb, PixelType.UnsignedByte, ref pixels);

        if (_renderers.Count == 0)
        {
            return;
        }


        GL.ReadPixels((int)Tofu.MouseInput.PositionInView.X * Screen.ScaleI,
            (int)Tofu.MouseInput.PositionInView.Y * Screen.ScaleI, 1, 1,
            PixelFormat.Rgba, PixelType.UnsignedByte, ref _currentPixel);

        // GL.Viewport();
        // GL.ReadPixels(idk, idk, 1, 1,
        // PixelFormat.Rgba, PixelType.UnsignedByte, ref _tempPixels);
    }

    // find renderer in Update, so we're not slowing down rendering/inflating the numbers
    public static void Update()
    {
        if (_currentPixel != _lastPixel)
        {
            _lastPixel = _currentPixel;
            HoveredRenderer = GetRenderer(_currentPixel); // only find renderer if we're hovering a different color
            // Color color = new Color(_pixels);
            Debug.Log($"picking pixel changed to {_currentPixel}");


            if (HoveredRenderer != null)
            {
                Debug.Log($"HoveredRenderer:{HoveredRenderer.GameObject.Name}");
            }
        }

        if (HoveredRenderer != null)
        {
            if (Tofu.MouseInput.ButtonPressed())
            {
                Debug.Log($"selected:{HoveredRenderer.GameObject.Name}");
                GameObjectSelectionManager.SelectGameObject(HoveredRenderer.GameObjectId);
            }
        }
        // Color color = new Color(_pixels);
        // Debug.StatSetValue("picking color", $"Picking hovered color:{color.ToString()}");
    }
}