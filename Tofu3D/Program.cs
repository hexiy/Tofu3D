using System.Globalization;
using System.Threading;
using OpenTK.Windowing.Common;
using Tofu3D.Tweening;

namespace Tofu3D;

public static class Program
{
	static void Main()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

		Global.LoadSavedData();
		_ = new Serializer();
		_ = new Scene();
		_ = new TweenManager();
		_ = new SceneNavigation();
		_ = new Editor();
		_ = new LightManager();

		AssetsWatcher.StartWatching();


		using Window window = new();

		window.Run();
	}
}