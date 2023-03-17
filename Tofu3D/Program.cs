using System.Globalization;
using System.IO;
using System.Threading;
using Tofu3D.Rendering;
using Tofu3D.Tweening;

namespace Tofu3D;

public static class Program
{
	static void Main()
	{
		
		// _ = new SceneSerializer();
		// _ = new Scene();
		// _ = new TweenManager();
		// _ = new SceneViewNavigation();
		// _ = new Editor();
		// _ = new LightManager();
		//
		// AssetsWatcher.StartWatching();

		Tofu tofu = new Tofu();
		tofu.Launch();
		/*Debug.StartTimer("Editor startup");
		using Window window = new();
		window.Run();*/
	}
}