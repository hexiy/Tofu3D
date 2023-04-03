using TextureLoader = Tofu3D.Components.TextureLoader;

public static class AssetManager
{
	static List<IAssetLoader> _loaders;

	static AssetManager()
	{
		TextureLoader textureLoader = new TextureLoader();
	}

	public static void AddAssetLoader(IAssetLoader assetLoader)
	{
		_loaders.Add(assetLoader);
	}

	public static T LoadAsset<T>(AssetLoadSettings<T> loadSettings = null) where T : Asset<T>, new()
	{
		T asset = new();
		asset.InitAssetHandle();


		return asset;
	}
}