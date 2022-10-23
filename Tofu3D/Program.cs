using OpenTK.Windowing.Common;
using Tofu3D.Tweening;

namespace Tofu3D;

public static class Program
{
	static void Main()
	{
		Global.LoadSavedData();
		_ = new Serializer();
		_ = new Scene();
		_ = new TweenManager();
		_ = new SceneNavigation();
		_ = new Editor();


		using Window window = new();
		
		window.Run();
	}
}