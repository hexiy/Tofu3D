using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

// Loads .asset files into runtime
public class AssetLoadManager
{
    private Dictionary<int, AssetBase> LoadedAssets { get; set; } = new(); // int is (raw asset)path hashcode

    public Dictionary<Type, Tuple<IAssetLoader, AssetLoadParametersBase>>
        LoadersAndLoadParameters { get; private set; } = new();


    public AssetLoadManager()
    {
        RegisterAssetLoader(new AssetLoader_Texture(), new AssetLoadParameters_Texture());
        RegisterAssetLoader(new AssetLoader_CubemapTexture(), new AssetLoadParameters_CubemapTexture());
        RegisterAssetLoader(new AssetLoader_Material(), new AssetLoadParameters_Material());
        RegisterAssetLoader(new AssetLoader_Model(), new AssetLoadParameters_Model());
        RegisterAssetLoader(new AssetLoader_Mesh(), new AssetLoadParameters_Mesh());
    }

    private void RegisterAssetLoader(IAssetLoader assetLoader, AssetLoadParametersBase assetLoadParameters)
    {
        LoadersAndLoadParameters.Add(assetLoader.GetType().BaseType.GenericTypeArguments[1],
            new Tuple<IAssetLoader, AssetLoadParametersBase>(assetLoader, assetLoadParameters));
    }


    public List<T> GetAllLoadedAssetsOfType<T>() where T : Asset<T>
    {
        List<T> foundAssets = new();
        var t = typeof(T);

        foreach (var keyValuePair in LoadedAssets)
        {
            if (keyValuePair.Value.RuntimeAssetHandle.AssetType == t)
            {
                foundAssets.Add(keyValuePair.Value as T);
            }
        }

        return foundAssets;
    }

    public T? GetLoadedAsset<T>(string sourcePath, AssetLoadParameters<T>? loadParameters = null) where T : Asset<T>
    {
        int id = sourcePath.GetHashCode();
        bool existsInDatabase = LoadedAssets.ContainsKey(id);

        // 
        T asset = null;

        if (existsInDatabase)
        {
            asset = LoadedAssets[id] as T;
        }

        return asset;
    }

    public bool IsAssetLoaded(string sourcePath)
    {
        int id = sourcePath.GetHashCode();
        bool existsInDatabase = LoadedAssets.ContainsKey(id);

        return existsInDatabase;
    }

    public T? Load<T>(AssetLoadParameters<T> loadParameters = null,
        bool overwriteAlreadyLoadedAssets = false, bool creatingRuntimeCopy = false) where T : Asset<T>
    {
        return Load<T>(loadParameters.PathToAsset, loadParameters, overwriteAlreadyLoadedAssets,
            creatingRuntimeCopy: creatingRuntimeCopy);
    }

    // path here will be Assets/xxxxx
    public T? Load<T>(string sourcePath, AssetLoadParameters<T>? loadParameters = null,
        bool overwriteAlreadyLoadedAssets = false, bool creatingRuntimeCopy = false) where T : Asset<T>
    {
        int id = sourcePath.GetHashCode();

        if (creatingRuntimeCopy)
        {
            id = -id; // temp only
            bool exists = LoadedAssets.ContainsKey(id);
            if (exists)
            {
                id = id - Random.Range(0, 1000000);
            }
        }

        bool existsInDatabase = LoadedAssets.ContainsKey(id);

        // 
        T asset = null;

        if (existsInDatabase && overwriteAlreadyLoadedAssets == false)
        {
            asset = LoadedAssets[id] as T;
        }
        else
        {
            if (LoadersAndLoadParameters.ContainsKey(typeof(T)) == false)
            {
                Debug.LogError("no loaders for this");
                return null;
            }

            Tuple<IAssetLoader, AssetLoadParametersBase> loaderAndLoadParameters = LoadersAndLoadParameters[typeof(T)];

            if (loadParameters == null)
            {
                loadParameters = (loaderAndLoadParameters.Item2 as AssetLoadParameters<T>);

                loadParameters =
                    Activator.CreateInstance(loadParameters.GetType()) as AssetLoadParameters<T>;

                loadParameters.PathToAsset = sourcePath.GetPathOfAssetInLibrayFromSourceAssetPathOrName();
            }

            if (File.Exists(sourcePath) == false)
            {
                Debug.LogError("not found asset " + loadParameters.PathToAsset);
                
                if (typeof(T) == typeof(Asset_Material))
                {
                    asset = CreateDefaultMaterialAssetFile(sourcePath) as T;
                }
            }
            else
            {
                asset = (T)((dynamic)loaderAndLoadParameters.Item1).LoadAsset(loadParameters);
            }

            LoadedAssets[id] = asset;
            // (loaderAndLoadParameters.Item1 as AssetLoader<T?,T>).LoadAsset(newInstanceOfLoadParameters);


            // i need the AssetLoadParameters

            // _assetDatabase.Assets[id] = asset;
        }

        return asset;
    }

    private Asset_Material CreateDefaultMaterialAssetFile(string sourcePath)
    {
        var mat = new Asset_Material()
            { Shader = new Shader("Assets/Shaders/ModelRendererInstanced.glsl") }; // default shader for now
        mat.LoadShader();
        mat.PathToRawAsset = sourcePath;
        mat.LoadTextures();
        QuickSerializer.SaveFileJSON<Asset_Material>(path: sourcePath, mat); // this needs to be here, otherwise there will be no .tofumaterial file in library if we're creating material ono the fly

        Tofu.AssetImportManager.ImportAsset(sourcePath,reimportIfExists:true);
        // QuickSerializer.SaveFileJSON<Asset_Material>(path: sourcePath, mat);
        // return mat as T;
        return mat;
    }

    public void Unload(string path)
    {
        int id = path.GetHashCode();
        if (LoadedAssets.ContainsKey(id))
        {
            // Debug.Log($"unloaded asset:{path}");
            // LoadedAssets[id].IsLoaded = false;
            LoadedAssets.Remove(id);
        }
    }

    public void Save<T>(string path, T asset, AssetLoadParameters<T>? loadParameters = null, bool json = true)
        where T : Asset<T>
    {
        // if (asset.IsRuntimeCopy)
        // {
        //     Debug.Log("Not saving runtime copy of asset");
        //     return;
        // }
        int id = path.GetHashCode();

        // 
        // T asset = null;

        if (json)
        {
            QuickSerializer.SaveFileJSON<T>(path, asset);
        }
        else
        {
            QuickSerializer.SaveFileXML<T>(path, asset);
        }

        LoadedAssets[id] = asset;
        Debug.Log($"Saved file {path}");
    }
    // public T Save<T>(T asset) where T : Asset<T>, new()
    // {
    //     var hash = settings.GetHashCode();
    //
    //
    //     // code in SaveAsset should use settings path to save the file not asset.path!!!
    //     (_loaders[typeof(T)] as AssetLoader<T>).SaveAsset(ref asset, settings);
    //
    //     Debug.StatSetValue("LoadedAssets", $"LoadedAssets:{_assets.Count}");
    //     // return _assets[hash] as T;
    //     return asset;
    // }

    // public void Unload<T>(Asset<T> asset, AssetLoadSettingsBase loadSettings) where T : Asset<T>
    // {
    //     var hash = loadSettings.GetHashCode();
    //
    //     if (_assets.ContainsKey(hash))
    //     {
    //         (_loaders[typeof(T)] as AssetLoader<T>).UnloadAsset(asset);
    //         _assets.Remove(hash);
    //     }
    //     else
    //     {
    //         Debug.Log("Cannot unload asset that's not loaded.");
    //         return;
    //     }
    //
    //     Debug.StatSetValue("LoadedAssets", $"LoadedAssets:{_assets.Count}");
    // }
}