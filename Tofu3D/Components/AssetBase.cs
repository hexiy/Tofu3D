namespace Tofu3D;

public abstract class AssetBase
{
	[XmlIgnore] public AssetRuntimeHandle AssetRuntimeHandle { get; set; }
	[Hide]
	public string AssetPath = "";

	// private Asset()
	// {
	// }

	public static implicit operator bool(AssetBase instance)
	{
		if (instance == null)
		{
			return false;
		}

		return true;
	}
}