using System.IO;

// Loads .asset files into runtime
public class AssetLoadManager
{
    private AssetLoader_RuntimeMesh _assetLoaderRuntimeMesh;

    private Dictionary<int, AssetBase> LoadedAssets { get; set; } =
        new Dictionary<int, AssetBase>(); // int is (raw asset)path hashcode

    public Dictionary<Type, Tuple<IAssetLoader, AssetLoadParametersBase>>
        LoadersAndLoadParameters { get; private set; } =
        new Dictionary<Type, Tuple<IAssetLoader, AssetLoadParametersBase>>();


    public AssetLoadManager()
    {
        _assetLoaderRuntimeMesh = new AssetLoader_RuntimeMesh();

        RegisterAssetLoader(new AssetLoader_RuntimeTexture(), new AssetLoadParameters_RuntimeTexture());
        RegisterAssetLoader(new AssetLoader_AssetTexture(), new AssetLoadParameters_AssetTexture());
        // RegisterAssetLoader(new AssetLoader_AtlasTexture(), new AssetLoadParameters_AtlasTexture());
        RegisterAssetLoader(new AssetLoader_CubemapTexture(), new AssetLoadParameters_CubemapTexture());
        RegisterAssetLoader(new AssetLoader_Material(), new AssetLoadParameters_Material());
        RegisterAssetLoader(new AssetLoader_Model(), new AssetLoadParameters_Model());
        RegisterAssetLoader(_assetLoaderRuntimeMesh, new AssetLoadParameters_RuntimeMesh());
        RegisterAssetLoader(new AssetLoader_MeshFile(), new AssetLoadParameters_MeshFile());

        Scene.SceneDisposed += UnloadALl;
    }

    private void RegisterAssetLoader(IAssetLoader assetLoader, AssetLoadParametersBase assetLoadParameters)
    {
        LoadersAndLoadParameters.Add(assetLoader.GetType().BaseType.GenericTypeArguments[0],
            new Tuple<IAssetLoader, AssetLoadParametersBase>(assetLoader, assetLoadParameters));
    }

    public void AddAsset<T>(AssetBase assetBase) where T : AssetBase
    {
        AddAsset(assetBase, typeof(T));
    }

    public void AddAsset(AssetBase assetBase, Type type)
    {
        if (assetBase == null)
        {
            return;
        }

        int id = GetAssetID(assetBase.PathInLibraryFolder, type);

        LoadedAssets[id] = assetBase;
    }

    public List<T> GetAllLoadedAssetsOfType<T>() where T : AssetBase
    {
        List<T> foundAssets = new List<T>();
        Type t = typeof(T);

        foreach (KeyValuePair<int, AssetBase> keyValuePair in LoadedAssets)
        {
            if (keyValuePair.Value?.GetType() == t)
            {
                foundAssets.Add(keyValuePair.Value as T);
            }
        }

        return foundAssets;
    }

    // public T? GetLoadedAsset<T>(string sourcePath, AssetLoadParameters<T>? loadParameters = null) where T : AssetBase
    // {
    //     int id = GetAssetID(sourcePath);
    //     bool existsInDatabase = LoadedAssets.ContainsKey(id);
    //
    //     // 
    //     T asset = null;
    //
    //     if (existsInDatabase)
    //     {
    //         asset = LoadedAssets[id] as T;
    //     }
    //
    //     return asset;
    // }

    public bool IsAssetLoaded<T>(string sourcePath, AssetLoadParametersBase? loadParameters = null) where T : AssetBase
    {
        int id = GetAssetID<T>(sourcePath, loadParameters);

        bool existsInDatabase = LoadedAssets.ContainsKey(id);

        return existsInDatabase;
    }

    /// <param name="original"></param>
    /// <param name="folder">By default is Library/Temp and gets wiped when Tofu is closed</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? CreateCopyFile<T>(T original, string? folder = null) where T : AssetBase
    {
        folder = folder ?? Folders.TempInLibrary;
        string tempFileName =
            Folders.GetPathRelativeToProjectFolder(
                TofuPath.Combine(folder, Guid.NewGuid().ToString()) + ".temp");

        Serializer.SaveFileJSON<T>(tempFileName, original);

        // for some reason   Tofu.AssetLoadManager.Save doesnt work it overwrites the assets....
        // Tofu.AssetLoadManager.Save<T>(tempFileName, asset: original);
        T runtimeCopy =
            Tofu.AssetLoadManager.Get<T>(tempFileName, null, false);
        runtimeCopy.PathInLibraryFolder = tempFileName;
        runtimeCopy.PathInAssetsFolder = null;
        Tofu.AssetLoadManager.Save<T>(tempFileName, runtimeCopy);
        // Debug.Log("Created new copy of asset");

        return runtimeCopy;
    }

    public T? CreateCopy<T>(T original) where T : Asset<T>
    {
        return original.Clone();
    }

    public T? Get<T>(AssetLoadParameters<T> loadParameters = null,
        bool overwriteAlreadyLoadedAssets = false, bool isRuntimeCopy = false) where T : AssetBase
    {
        return Get<T>(loadParameters.PathToAssetInLibrary, loadParameters, overwriteAlreadyLoadedAssets,
            isRuntimeCopy: isRuntimeCopy);
    }

    // path here will be Assets/xxxxx
    public T? Get<T>(string sourcePath, AssetLoadParameters<T>? loadParameters = null,
        bool overwriteAlreadyLoadedAssets = false, bool isRuntimeCopy = false) where T : AssetBase
    {
        string pathToAssetInLibrary =
            AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(sourcePath);

        int id = GetAssetID<T>(sourcePath, loadParameters);

        if (isRuntimeCopy)
        {
            id = -Math.Abs(id); // temp only
            bool exists = LoadedAssets.ContainsKey(id);
            // if (exists)
            // {
            // id = Random.Range(int.MinValue, -1);
            // }
        }

        bool existsInDatabase = LoadedAssets.ContainsKey(id);

        // 
        T asset = null;

        if (existsInDatabase && overwriteAlreadyLoadedAssets == false)
        {
            asset = LoadedAssets[id] as T;
            return asset;
        }
        else
        {
            if (LoadersAndLoadParameters.ContainsKey(typeof(T)) == false)
            {
                Debug.LogError("no loaders for this");
                return null;
            }

            Tuple<IAssetLoader, AssetLoadParametersBase> loaderAndLoadParameters;
            bool foundLoader = LoadersAndLoadParameters.TryGetValue(typeof(T), out loaderAndLoadParameters);

            if (loadParameters == null)
            {
                // Tuple<IAssetLoader, AssetLoadParametersBase> loaderAndLoadParameters = LoadersAndLoadParameters[typeof(T)];
                if (foundLoader && loaderAndLoadParameters.Item2 != null)
                {
                    loadParameters = (loaderAndLoadParameters.Item2 as AssetLoadParameters<T>);

                    loadParameters =
                        Activator.CreateInstance(loadParameters.GetType()) as AssetLoadParameters<T>;

                    loadParameters.PathToAssetInLibrary = pathToAssetInLibrary;
                    if (File.Exists(pathToAssetInLibrary) == false)
                    {
                        loadParameters.PathToAssetInLibrary = sourcePath;
                    }
                }
            }

            if (existsInDatabase)
            {
                T existingAsset = LoadedAssets[id] as T;
                // problem is this is Asset_Texture not RuntimeTexture
                // loadedassets has only asset_textures right? not runtimetextures
                if (loadParameters != null)
                {
                    loadParameters.ExistingAsset = existingAsset;
                }
            }

            // because in EditorTextures its null first so we overwrite it but it stays that value even after so all icons are the same as the first one 
            bool setLoadParametersPathBackToNull = false;
            if (loadParameters is { PathToAssetInLibrary: null })
            {
                setLoadParametersPathBackToNull = true;
                loadParameters.PathToAssetInLibrary = sourcePath;
            }

            if (typeof(T) == typeof(Asset_Material) && File.Exists(sourcePath) == false)
            {
                asset = CreateDefaultMaterialAssetFile(sourcePath) as T;
            }
            else
            {
                asset = (T)((dynamic)loaderAndLoadParameters.Item1).LoadAsset(loadParameters);
            }

            LoadedAssets[id] = asset;
            // (loaderAndLoadParameters.Item1 as AssetLoader<T?,T>).LoadAsset(newInstanceOfLoadParameters);


            // i need the AssetLoadParameters

            // _assetDatabase.Assets[id] = asset;

            if (setLoadParametersPathBackToNull)
            {
                loadParameters.PathToAssetInLibrary = null;
            }
        }

        return asset;
    }

    public RuntimeMesh LoadRuntimeMeshFromAssetMesh<T>(MeshFile meshFile,
        AssetLoadParameters<RuntimeMesh>? loadParameters = null) where T : AssetBase
    {
        RuntimeMesh runtimeMesh = _assetLoaderRuntimeMesh.LoadAsset(meshFile: meshFile, loadParameters);
        return runtimeMesh;
    }

    private Asset_Material CreateDefaultMaterialAssetFile(string sourcePath)
    {
        Asset_Material mat = new Asset_Material()
        {
            Shader = Tofu.ShaderManager.LoadShader(TofuPath.Combine(Folders.ShadersInAssets,
                "ModelRendererInstanced.glsl"))
        }; // default shader for now
        mat.LoadShader();
        mat.PathInAssetsFolder = sourcePath;
        mat.LoadTextures();
        Serializer.SaveAssetJSON<Asset_Material>(path: sourcePath,
            mat); // this needs to be here, otherwise there will be no .tofumaterial file in library if we're creating material ono the fly

        Tofu.AssetImportManager.ImportAsset(sourcePath, reimportIfExists: true);
        // QuickSerializer.SaveFileJSON<Asset_Material>(path: sourcePath, mat);
        // return mat as T;
        return mat;
    }

    public void Unload<T>(string sourcePath, AssetLoadParametersBase? loadParameters) where T : AssetBase
    {
        int id = GetAssetID<T>(sourcePath, loadParameters);
        if (LoadedAssets.ContainsKey(id))
        {
            // Debug.Log($"unloaded asset:{path}");
            // LoadedAssets[id].IsLoaded = false;
            LoadedAssets.Remove(id);
        }
    }

    public void UnloadALl()
    {
        LoadedAssets.Clear();
    }

    public void Save<T>(string path, T asset) where T : AssetBase
    {
        int id = GetAssetID<T>(path, asset.AssetLoadParameters, asset.AssetImportParameters); //, typeof(T));

        Serializer.SaveAssetJSON<T>(path, asset);

        LoadedAssets[id] = asset;
        // Debug.Log($"Saved file {path}");
    }

    // private int GetAssetID<T>(string path) where T : AssetBase

    private int GetAssetID<T>(string path, AssetLoadParametersBase? loadParameters = null,
        AssetImportParametersBase? importParameters = null) where T : AssetBase
    {
        return GetAssetID(path: path, type: typeof(T));
    }

    private int GetAssetID(string path, Type type, AssetLoadParametersBase? loadParameters = null,
        AssetImportParametersBase? importParameters = null)
    {
        string pathToAssetInLibrary =
            AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(path);

        int id = (pathToAssetInLibrary + type.ToString() + (loadParameters?.GetHashCode() ?? 0) +
                  (importParameters?.GetHashCode() ?? 0)).GetHashCode();
        // int id = (pathToAssetInLibrary).GetHashCode();
        return id;
    }
}