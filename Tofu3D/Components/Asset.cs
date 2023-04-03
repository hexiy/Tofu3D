namespace Tofu3D;

public class Asset<T> where T : Asset<T> //, new()
{
	public AssetHandle<T> AssetHandle { get; private set; }
	public string Path = "";

	// private Asset()
	// {
	// }

	public void InitAssetHandle()
	{
		AssetHandle = new AssetHandle<T>();
	}
	// public int Hash
	// {
	// 	get { return Id.GetHashCode(); }
	// }
}