namespace Tofu3D;

public abstract class AssetBase
{
	[XmlIgnore] public AssetHandle Handle { get; set; }
	[Hide]
	[XmlElement("AssetPath")]
	public string Path = "";

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