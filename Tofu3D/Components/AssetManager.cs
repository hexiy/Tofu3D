public static class AssetManager
{
	static Dictionary<Type, IAssetLoader> _loaders = new Dictionary<Type, IAssetLoader>();
	static Dictionary<Type, Type> _loadSettingsTypes = new Dictionary<Type, Type>();

	static AssetManager()
	{
		TextureLoader textureLoader = new();
		AddAssetLoader(textureLoader);

		ModelLoader modelLoader = new();
		AddAssetLoader(modelLoader);

		_loadSettingsTypes.Add(typeof(Texture), typeof(TextureLoadSettings));
	}

	private static void AddAssetLoader(IAssetLoader assetLoader)
	{
		_loaders.Add(assetLoader.GetType().BaseType.GenericTypeArguments[0], assetLoader);
	}

	public static T Load<T>(string path, IAssetLoadSettings? loadSettings = null) where T : Asset<T>, new()
	{
		// AssetLoadSettings<T> loadSettings = new();

		AssetLoadSettings<T> settings = (loadSettings as AssetLoadSettings<T>) ?? Activator.CreateInstance(_loadSettingsTypes[typeof(T)]) as AssetLoadSettings<T>;
		settings.Path = path;
		return Load<T>(loadSettings: settings);
	}

	public static T Load<T>(IAssetLoadSettings loadSettings) where T : Asset<T>, new()
	{
		T asset = new();

		string assetPath = (loadSettings as AssetLoadSettings<T>).Path;
		int hash = assetPath.GetHashCode();
		// asset.InitAssetHandle(hash);

		asset = (_loaders[typeof(T)] as AssetLoader<T>).LoadAsset(loadSettings: loadSettings) as T;
		// asset type specifics


		return asset;
	}
}