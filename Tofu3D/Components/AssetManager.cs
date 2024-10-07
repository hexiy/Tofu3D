﻿using System.IO;

public class AssetManager
{
    private readonly Dictionary<int, AssetBase> _assets = new();
    private readonly Dictionary<Type, IAssetLoader> _loaders = new();
    private readonly Dictionary<Type, Type> _loadSettingsTypes = new();

    public AssetManager()
    {
        AddAssetLoader(new TextureLoader());
        AddAssetLoader(new ModelLoader());
        AddAssetLoader(new MaterialLoader());
        AddAssetLoader(new CubemapTextureLoader());


        foreach (var loadSettingsType in new[]
                 {
                     typeof(TextureLoadSettings),
                     typeof(ModelLoadSettings),
                     typeof(MaterialLoadSettings),
                     typeof(CubemapTextureLoadSettings)
                 })
        {
            _loadSettingsTypes.Add(loadSettingsType.BaseType.GenericTypeArguments[0], loadSettingsType);
        }
    }

    public void AddLoadSettingsType(Type type)
    {
        _loadSettingsTypes.Add(type.BaseType.GenericTypeArguments[0], type);
    }

    private void AddAssetLoader(IAssetLoader assetLoader)
    {
        _loaders.Add(assetLoader.GetType().BaseType.GenericTypeArguments[0], assetLoader);
    }

    public List<T> GetAllLoadedAssetsOfType<T>() where T : Asset<T>
    {
        List<T> foundAssets = new();
        var t = typeof(T);

        foreach (var keyValuePair in _assets)
        {
            if (keyValuePair.Value.Handle.AssetType == t)
            {
                foundAssets.Add(keyValuePair.Value as T);
            }
        }

        return foundAssets;
    }

    public T Load<T>(string path, AssetLoadSettingsBase? loadSettings = null) where T : Asset<T>
    {
        // AssetLoadSettings<T> loadSettings = new();

        var settings = loadSettings as AssetLoadSettings<T> ??
                       Activator.CreateInstance(_loadSettingsTypes[typeof(T)]) as AssetLoadSettings<T>;
        settings.Path = path;
        return Load<T>(settings);
    }

    public T Load<T>(AssetLoadSettingsBase loadSettings) where T : Asset<T>
    {
        // now, we need to check if the asset has already been created, do we get the .meta file? or ust check in assetDatabase if the filepath corresponds 
        // 
        T asset;
        loadSettings.ValidatePath();
        var hash = loadSettings.GetHashCode();

        if (_assets.ContainsKey(hash))
        {
            asset = _assets[hash] as T;
        }
        else
        {
            asset = (_loaders[typeof(T)] as AssetLoader<T>).LoadAsset(loadSettings) as T;
            _assets[hash] = asset;
            // Debug.Log($"Loaded asset:{assetPath}");
        }

        Debug.StatSetValue("LoadedAssets", $"LoadedAssets:{_assets.Count}");
        return asset;
    }

    public T Save<T>(T asset, AssetLoadSettingsBase loadSettings = null) where T : Asset<T>, new()
    {
        var settings = loadSettings as AssetLoadSettings<T> ??
                       Activator.CreateInstance(_loadSettingsTypes[typeof(T)]) as AssetLoadSettings<T>;
        settings.Path = asset.Path;
        settings.ValidatePath();
        // if (File.Exists(settings.Path) == false)
        // {
        //     Debug.LogError("Cannot save asset, no path present");
        //     return null;
        // }


        var hash = settings.GetHashCode();


        // code in SaveAsset should use settings path to save the file not asset.path!!!
        (_loaders[typeof(T)] as AssetLoader<T>).SaveAsset(ref asset, settings);

        Debug.StatSetValue("LoadedAssets", $"LoadedAssets:{_assets.Count}");
        // return _assets[hash] as T;
        return asset;
    }

    public void Unload<T>(Asset<T> asset, AssetLoadSettingsBase loadSettings) where T : Asset<T>
    {
        var hash = loadSettings.GetHashCode();

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