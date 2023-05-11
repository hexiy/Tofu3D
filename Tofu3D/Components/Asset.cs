using System.Runtime.Serialization;

namespace Tofu3D;

[Serializable]
public abstract class Asset<T> : AssetBase where T : Asset<T> //, new()
{
	public void InitAssetRuntimeHandle(int id)
	{
		AssetRuntimeHandle = new AssetRuntimeHandle() {Id = id, AssetType = typeof(T)};
	}
}