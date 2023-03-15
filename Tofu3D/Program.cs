using System.Globalization;
using System.IO;
using System.Threading;
using Engine;
using Tofu3D.Rendering;
using Tofu3D.Tweening;

namespace Tofu3D;

public static class Program
{
	static void Main()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

		Environment.CurrentDirectory = Directory.GetParent(Folders.Assets).FullName;
		
		Global.LoadSavedData();
		_ = new Serializer();
		_ = new Scene();
		_ = new TweenManager();
		_ = new SceneNavigation();
		_ = new Editor();
		_ = new LightManager();

		AssetsWatcher.StartWatching();

		Debug.StartTimer("Editor startup");
		using Window window = new();

		window.Run();
	}
}