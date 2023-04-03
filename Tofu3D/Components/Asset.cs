using System.Runtime.Serialization;

namespace Tofu3D;

public class Asset<T> :IAsset where T : Asset<T> //, new()
{
	public AssetHandle AssetHandle { get; set; }
	public string AssetPath = "";

	// private Asset()
	// {
	// }

	public void InitAssetHandle(int id)
	{
		AssetHandle = new AssetHandle() {Id = id, AssetType = typeof(T)};
	}
}