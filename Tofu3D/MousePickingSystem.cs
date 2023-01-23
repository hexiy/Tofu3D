using System.Threading.Tasks;
using Tofu3D.Rendering;

namespace Engine;

public static class MousePickingSystem
{
	static HashSet<MousePickingObject> _renderers = new HashSet<MousePickingObject>();
	static uint _pixels;
	public static Renderer HoveredRenderer { get; private set; }

	public static Color RegisterObject(Renderer renderer)
	{
		MousePickingObject mousePickingObject = new MousePickingObject() {Renderer = renderer, Color = GetFreeColor()};
		_renderers.Add(mousePickingObject);
		return new Color(mousePickingObject.Color);
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
		foreach (MousePickingObject mousePickingObject in _renderers)
		{
			if (mousePickingObject.Color == color)
			{
				return mousePickingObject.Renderer;
			}
		}

		return null;
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
		if (MouseInput.ScreenDelta == Vector2.Zero || SceneNavigation.I.IsPanningCamera)
		{
			return;
		}

		GL.ReadPixels((int) MouseInput.ScreenPosition.X, (int) MouseInput.ScreenPosition.Y, 1, 1, PixelFormat.Rgb, PixelType.UnsignedByte, ref _pixels);
		//Color color = new Color(_pixels);


		HoveredRenderer = GetRenderer(_pixels);
		Debug.Log($"HoveredRenderer:{HoveredRenderer?.GameObject.Name}");
	}
}