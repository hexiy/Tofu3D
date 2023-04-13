using System.Runtime.Serialization;

namespace Tofu3D;

[Serializable]
public class Asset<T> : AssetBase, IInspectable where T : Asset<T> //, new()
{
	[XmlIgnore] public AssetHandle AssetHandle { get; set; }
	public string AssetPath = "";

	// private Asset()
	// {
	// }

	public void InitAssetHandle(int id)
	{
		AssetHandle = new AssetHandle() {Id = id, AssetType = typeof(T)};
	}

	public static implicit operator bool(Asset<T> instance)
	{
		if (instance == null)
		{
			return false;
		}

		return true;
	}
}