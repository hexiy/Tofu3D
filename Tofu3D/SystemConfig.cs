using System.Globalization;
using System.IO;
using System.Threading;

namespace Tofu3D;

public static class SystemConfig
{
	public static void Configure()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

		Environment.CurrentDirectory = Directory.GetParent(Folders.Assets).FullName;
	}
}