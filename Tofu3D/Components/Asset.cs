using System.Runtime.Serialization;

namespace Tofu3D;

public class Asset<T> where T : Asset<T> //, new()
{
	public AssetHandle<T> AssetHandle { get; set; }
	public string AssetPath = "";

	// private Asset()
	// {
	// }

	public void InitAssetHandle(uint id)
	{
		AssetHandle = new AssetHandle<T>() {Id = id};
	}
}