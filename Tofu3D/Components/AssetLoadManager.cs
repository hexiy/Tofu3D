using System.IO;

// Loads .asset files into runtime
public class AssetLoadManager
{
    private AssetLoader_RuntimeMesh _assetLoaderRuntimeMesh;
    private Dictionary<int, object> LoadedAssets { get; set; } = new Dictionary<int, object>(); // int is (raw asset)path hashcode

    public Dictionary<Type, Tuple<IAssetLoader, AssetLoadParametersBase>>
        LoadersAndLoadParameters { get; private set; } =
        new Dictionary<Type, Tuple<IAssetLoader, AssetLoadParametersBase>>();


    public AssetLoadManager()
    {
        _assetLoaderRuntimeMesh = new AssetLoader_RuntimeMesh();

        RegisterAssetLoader(new AssetLoader_Texture(), new AssetLoadParameters_Texture());
        // RegisterAssetLoader(new AssetLoader_AtlasTexture(), new AssetLoadParameters_AtlasTexture());
        RegisterAssetLoader(new AssetLoader_CubemapTexture(), new AssetLoadParameters_CubemapTexture());
        RegisterAssetLoader(new AssetLoader_Material(), new AssetLoadParameters_Material());
        RegisterAssetLoader(new AssetLoader_Model(), new AssetLoadParameters_Model());
        RegisterAssetLoader(new AssetLoader_RuntimeMesh(), new AssetLoadParameters_RuntimeMesh());
        RegisterAssetLoader(new AssetLoader_AssetMesh(), new AssetLoadParameters_AssetMesh());

        Scene.SceneDisposed += UnloadALl;
    }

    private void RegisterAssetLoader(IAssetLoader assetLoader, AssetLoadParametersBase assetLoadParameters)
    {
        LoadersAndLoadParameters.Add(assetLoader.GetType().BaseType.GenericTypeArguments[0],
            new Tuple<IAssetLoader, AssetLoadParametersBase>(assetLoader, assetLoadParameters));
    }


    public List<T> GetAllLoadedAssetsOfType<T>() where T : Asset<T>
    {
        List<T> foundAssets = new List<T>();
        Type t = typeof(T);

        foreach (KeyValuePair<int, object> keyValuePair in LoadedAssets)
        {
            if (keyValuePair.Value?.GetType() == t)
            {
                foundAssets.Add(keyValuePair.Value as T);
            }
        }

        return foundAssets;
    }

    public T? GetLoadedAsset<T>(string sourcePath, AssetLoadParameters<T>? loadParameters = null) where T : Asset<T>
    {
        int id = (sourcePath + typeof(T)).GetHashCode();
        bool existsInDatabase = LoadedAssets.ContainsKey(id);

        // 
        T asset = null;

        if (existsInDatabase)
        {
            asset = LoadedAssets[id] as T;
        }

        return asset;
    }

    public bool IsAssetLoaded<T>(string sourcePath) where T : class
    {
        int id = (sourcePath + typeof(T)).GetHashCode();
        bool existsInDatabase = LoadedAssets.ContainsKey(id);

        return existsInDatabase;
    }

    /// <param name="original"></param>
    /// <param name="folder">By default is Library/Temp and gets wiped when Tofu is closed</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? CreateCopyFile<T>(T original, string? folder = null) where T : Asset<T>
    {
        folder = folder ?? Folders.TempInLibrary;
        string tempFileName =
            Folders.GetPathRelativeToProjectFolder(
                TofuPath.Combine(folder, Guid.NewGuid().ToString()) + ".temp");

        Serializer.SaveFileJSON<T>(tempFileName, original);

        // for some reason   Tofu.AssetLoadManager.Save doesnt work it overwrites the assets....
        // Tofu.AssetLoadManager.Save<T>(tempFileName, asset: original);
        T runtimeCopy =
            Tofu.AssetLoadManager.Load<T>(tempFileName, null, false);
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

    public T? Load<T>(AssetLoadParameters<T> loadParameters = null,
        bool overwriteAlreadyLoadedAssets = false, bool isRuntimeCopy = false) where T : class
    {
        return Load<T>(loadParameters.PathToAssetInLibrary, loadParameters, overwriteAlreadyLoadedAssets,
            isRuntimeCopy: isRuntimeCopy);
    }

    // path here will be Assets/xxxxx
    public T? Load<T>(string sourcePath, AssetLoadParameters<T>? loadParameters = null,
        bool overwriteAlreadyLoadedAssets = false, bool isRuntimeCopy = false) where T : class
    {
        int id = (sourcePath + typeof(T)).GetHashCode();

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

                loadParameters.PathToAssetInLibrary =
                    AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(sourcePath);
                if (File.Exists(loadParameters.PathToAssetInLibrary) == false)
                {
                    loadParameters.PathToAssetInLibrary = sourcePath;
                }
            }

            if (existsInDatabase)
            {
                T existingAsset = LoadedAssets[id] as T;
                // problem is this is Asset_Texture not RuntimeTexture
                // loadedassets has only asset_textures right? not runtimetextures
                loadParameters.ExistingAsset = existingAsset;
            }

            // because in EditorTextures its null first so we overwrite it but it stays that value even after so all icons are the same as the first one 
            bool setLoadParametersPathBackToNull = false;
            if (loadParameters.PathToAssetInLibrary == null)
            {
                setLoadParametersPathBackToNull = true;
                loadParameters.PathToAssetInLibrary = sourcePath;
            }

            if (File.Exists(sourcePath) == false)
            {
                Debug.LogWarning("not found asset " + loadParameters.PathToAssetInLibrary);

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

            if (setLoadParametersPathBackToNull)
            {
                loadParameters.PathToAssetInLibrary = null;
            }
        }

        return asset;
    }

    public RuntimeMesh LoadRuntimeMeshFromAssetMesh<T>(MeshFile meshFile,
        AssetLoadParameters<RuntimeMesh>? loadParameters = null) where T : Asset<T>
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

    public void Unload<T>(string path) where T : Asset<T>
    {
        int id = path.GetHashCode() + typeof(T).GetHashCode();
        if (LoadedAssets.ContainsKey(id))
        {
            // Debug.Log($"unloaded asset:{path}");
            // LoadedAssets[id].IsLoaded = false;
            LoadedAssets.Remove(id);
        }
    }

    public void UnloadALl()
    {
        LoadedAssets = new Dictionary<int, object>();
    }

    public void Save<T>(string path, T asset)
        where T : class
    {
        int id = (path + typeof(T)).GetHashCode();

        Serializer.SaveFileJSON<T>(path, asset);

        LoadedAssets[id] = asset;
        // Debug.Log($"Saved file {path}");
    }
}