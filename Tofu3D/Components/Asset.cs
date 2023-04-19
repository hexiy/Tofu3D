using System.Runtime.Serialization;

namespace Tofu3D;

[Serializable]
public abstract class Asset<T> : AssetBase, IInspectable where T : Asset<T> //, new()
{
	[XmlIgnore] public AssetRuntimeHandle AssetRuntimeHandle { get; set; }
	public string AssetPath = "";

	// private Asset()
	// {
	// }

	public void InitAssetRuntimeHandle(int id)
	{
		AssetRuntimeHandle = new AssetRuntimeHandle() {Id = id, AssetType = typeof(T)};
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