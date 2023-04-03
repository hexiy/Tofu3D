public static class AssetManager
{
	static Dictionary<Type, IAssetLoader> _loaders = new Dictionary<Type, IAssetLoader>();
	static Dictionary<Type, Type> _loadSettingsTypes = new Dictionary<Type, Type>();

	static Dictionary<int, IAsset> _assets = new Dictionary<int, IAsset>();

	static AssetManager()
	{
		TextureLoader textureLoader = new();
		AddAssetLoader(textureLoader);

		ModelLoader modelLoader = new();
		AddAssetLoader(modelLoader);


		foreach (Type loadSettingsType in new[]
		                                  {
			                                  typeof(TextureLoadSettings),
		                                  })
		{
			_loadSettingsTypes.Add(loadSettingsType.BaseType.GenericTypeArguments[0], loadSettingsType);
		}
	}

	public static void AddLoadSettingsType(Type type)
	{
		_loadSettingsTypes.Add(type.BaseType.GenericTypeArguments[0], type);
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
		// now, we need to check if the asset has already been created, do we get the .meta file? or ust check in assetDatabase if the filepath corresponds 
		// 
		T asset;

		string assetPath = (loadSettings as AssetLoadSettings<T>).Path;
		int hash = assetPath.GetHashCode();

		if (_assets.ContainsKey(hash) == false)
		{
			asset = (_loaders[typeof(T)] as AssetLoader<T>).LoadAsset(loadSettings: loadSettings) as T;

			_assets.Add(hash, asset);
		}
		else
		{
			asset = _assets[hash] as T;
		}

		return asset;
	}
}