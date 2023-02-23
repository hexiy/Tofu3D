using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Tofu3D.Rendering;

namespace Engine;

public static class MousePickingSystem
{
	static Dictionary<uint, Renderer> _renderers = new Dictionary<uint, Renderer>();
	static uint _pixels;
	public static Renderer HoveredRenderer { get; private set; }

	public static Color RegisterObject(ModelRenderer renderer)
	{
		// MousePickingObject mousePickingObject = new MousePickingObject() {Renderer = renderer, Color = GetFreeColor()};
		uint col = GetFreeColor();
		_renderers[col] = renderer;
		// _renderers.Add(mousePickingObject);
		return new Color(col);
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
		return (uint) _renderers.Count;
		// int r = (int) Mathf.ClampMax(_renderers.Count, 255);
		// int g = (int) Mathf.ClampMax(_renderers.Count % 255 - r, 255);
		// int b = (int) Mathf.ClampMax((_renderers.Count % 255) % 255 - r - g, 255);
		// return new Color(r, g, b, 0);
	}

	public static void Initialize()
	{
		//_renderers = new HashSet<MousePickingObject>();
		RenderPassSystem.RegisterRender(RenderPassType.MousePicking, RenderPassMousePicking);
	}

	static void RenderPassMousePicking()
	{
		Scene.I.RenderScene();
	}

	public static void Update()
	{
		// MousePickingSystem.Update();

		// GL.ReadPixels(0,0,1,1,PixelFormat.Rgb, PixelType.UnsignedByte, ref pixels);
		
		// need to update even when not moving mouse because objects can move in the scene
		if (MouseInput.ScreenDelta == Vector2.Zero || SceneNavigation.I.IsPanningCamera)
		{
			return;
		}

		GL.ReadPixels((int) MouseInput.ScreenPosition.X, (int) MouseInput.ScreenPosition.Y, 1, 1, PixelFormat.Rgb, PixelType.UnsignedByte, ref _pixels);
		//Color color = new Color(_pixels);


		HoveredRenderer = GetRenderer(_pixels);
		// Debug.Log($"HoveredRenderer:{HoveredRenderer?.GameObject.Name}");
	}
}