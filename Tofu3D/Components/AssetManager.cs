public static class AssetManager
{
	static Dictionary<Type, IAssetLoader> _loaders = new Dictionary<Type, IAssetLoader>();
	static Dictionary<Type, Type> _loadSettingsTypes = new Dictionary<Type, Type>();

	static Dictionary<int, AssetBase> _assets = new Dictionary<int, AssetBase>();

	static AssetManager()
	{
		AddAssetLoader(new TextureLoader());
		AddAssetLoader(new ModelLoader());
		AddAssetLoader(new MaterialLoader());


		foreach (Type loadSettingsType in new[]
		                                  {
			                                  typeof(TextureLoadSettings),
			                                  typeof(ModelLoadSettings),
			                                  typeof(MaterialLoadSettings)
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

	public static T Load<T>(string path, AssetLoadSettingsBase? loadSettings = null) where T : Asset<T>, new()
	{
		// AssetLoadSettings<T> loadSettings = new();

		AssetLoadSettings<T> settings = (loadSettings as AssetLoadSettings<T>) ?? Activator.CreateInstance(_loadSettingsTypes[typeof(T)]) as AssetLoadSettings<T>;
		settings.Path = path;
		return Load<T>(loadSettings: settings);
	}

	public static T Load<T>(AssetLoadSettingsBase loadSettings) where T : Asset<T>, new()
	{
		// now, we need to check if the asset has already been created, do we get the .meta file? or ust check in assetDatabase if the filepath corresponds 
		// 
		T asset;

		string assetPath = loadSettings.Path;
		int hash = loadSettings.GetHashCode();

		if (_assets.ContainsKey(hash))
		{
			asset = _assets[hash] as T;
		}
		else
		{
			asset = (_loaders[typeof(T)] as AssetLoader<T>).LoadAsset(loadSettings: loadSettings) as T;
			_assets.Add(hash, asset);
			// Debug.Log($"Loaded asset:{assetPath}");
		}

		Debug.StatSetValue("LoadedAssets", $"LoadedAssets:{_assets.Count}");
		return asset;
	}

	public static T Save<T>(T asset, AssetLoadSettingsBase loadSettings = null) where T : Asset<T>, new()
	{
		// string assetPath = (loadSettings as AssetLoadSettings<T>).Path;
		AssetLoadSettings<T> settings = (loadSettings as AssetLoadSettings<T>) ?? Activator.CreateInstance(_loadSettingsTypes[typeof(T)]) as AssetLoadSettings<T>;
		settings.Path = asset.AssetPath;
		int hash = settings.GetHashCode();

		(_loaders[typeof(T)] as AssetLoader<T>).SaveAsset(asset, loadSettings);

		Debug.StatSetValue("LoadedAssets", $"LoadedAssets:{_assets.Count}");
		return asset;
	}

	public static void Unload<T>(Asset<T> asset) where T : Asset<T>
	{
		int hash = asset.AssetPath.GetHashCode();

		if (_assets.ContainsKey(hash))
		{
			(_loaders[typeof(T)] as AssetLoader<T>).UnloadAsset(asset);
			_assets.Remove(hash);
		}
		else
		{
			Debug.Log("Cannot unload asset that's not loaded.");
			return;
		}

		Debug.StatSetValue("LoadedAssets", $"LoadedAssets:{_assets.Count}");
	}
}