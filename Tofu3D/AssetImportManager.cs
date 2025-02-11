using System.IO;
using System.Threading;

namespace Tofu3D;

// Transforms .obj,.png files into .asset files in /Library/
public class AssetImportManager
{
    public Dictionary<int, AssetImportParametersBase> AssetImportParameters { get; private set; } =
        new Dictionary<int, AssetImportParametersBase>();

    public Dictionary<Type, IAssetImporter> Importers { get; private set; } = new Dictionary<Type, IAssetImporter>();

    public AssetImportManager()
    {
        RegisterAssetImporter(new AssetImporter_Model());
        RegisterAssetImporter(new AssetImporter_Texture());
        RegisterAssetImporter(new AssetImporter_Material());
    }

    private void RegisterAssetImporter(IAssetImporter assetImporter)
    {
        Importers.Add(assetImporter.GetType().BaseType.GenericTypeArguments[0], assetImporter);
    }

    private void ImportAssetInNewThread(string rawAssetPath, bool reimportIfExists = false)
    {
        rawAssetPath = AssetPathConverter.ToProjectRelativePath(rawAssetPath);
        int id = rawAssetPath.GetHashCode();
        // Tofu.AssetLoadManager.Unload(rawAssetPath);

        string rawAssetFileName = Path.GetFileName(rawAssetPath); // with extension

        string importParametersFilePath = AssetPathExtensions.GetPathOfImportParametersOfSourceAssetFile(rawAssetPath);
        string assetFileInLibraryPath =
            AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(rawAssetFileName);
        bool assetExists =
            AssetFileExists(
                AssetPathExtensions.GetPathOfAssetInLibraryFromSourceAssetPathOrName(assetFileInLibraryPath));
        bool canImport = assetExists == false || reimportIfExists == true;

        if (assetExists && canImport == false)
        {
            if (AssetPathExtensions.IsAnyAssetBase(assetFileInLibraryPath))
            {
                AssetBase asset = null;
                Type assetType = null;
                if (AssetPathExtensions.IsFileModel(rawAssetPath))
                {
                    asset = Serializer.ReadAssetJSON<Asset_Model>(assetFileInLibraryPath);
                    assetType = typeof(Asset_Model);
                }

                if (AssetPathExtensions.IsFileTexture(rawAssetPath))
                {
                    asset = Serializer.ReadAssetJSON<Asset_Texture>(assetFileInLibraryPath);
                    assetType = typeof(Asset_Texture);
                }

                if (AssetPathExtensions.IsFileTextureAtlas(rawAssetPath))
                {
                    asset = Serializer.ReadAssetJSON<Asset_TextureAtlas>(assetFileInLibraryPath);
                    assetType = typeof(Asset_TextureAtlas);
                }

                if (asset != null)
                {
                    Tofu.AssetLoadManager.AddAsset(asset, assetType);
                    // Tofu.AssetFileCache.AddAsset(asset);
                }
            }
        }

        if (canImport == false)
        {
            return;
        }


        bool assetImportParametersFileExistsForThisAsset = File.Exists(importParametersFilePath);
        const bool FORCE_NEW_IMPORT_PARAMETERS = false;
        if (FORCE_NEW_IMPORT_PARAMETERS)
        {
            assetImportParametersFileExistsForThisAsset = false;
        }

        if (AssetPathExtensions.IsFileModel(rawAssetPath))
        {
            AssetImportParameters_Model assetImportParametersModel;
            if (assetImportParametersFileExistsForThisAsset == false)
            {
                assetImportParametersModel = new AssetImportParameters_Model
                {
                    PathToSourceAsset = rawAssetPath
                };

                Serializer.SaveFileJSON<AssetImportParameters_Model>(importParametersFilePath,
                    assetImportParametersModel);
            }
            else
            {
                assetImportParametersModel =
                    Serializer.ReadFileJSON<AssetImportParameters_Model>(importParametersFilePath);
            }

            AssetImportParameters[id] = assetImportParametersModel;

            if (canImport)
            {
                Asset_Model model = (Importers[typeof(Asset_Model)] as AssetImporter_Model)
                    .ImportAsset(assetImportParametersModel);

                foreach (string meshAsset in model.PathsToMeshAssets)
                {
                    // if mesh was loaded, we load new mesh
                    if (Tofu.AssetLoadManager.IsAssetLoaded<RuntimeMesh>(meshAsset))
                    {
                        // Tofu.AssetLoadManager.Unload(meshAsset);
                        Tofu.AssetLoadManager.Get<RuntimeMesh>(meshAsset, overwriteAlreadyLoadedAssets: true);
                    }
                }

                // Assets[id] = model;
            }
        }

        if (AssetPathExtensions.IsFileMaterial(rawAssetPath))
        {
            AssetImportParameters_Material assetImportParametersMaterial = new AssetImportParameters_Material
            {
                // if (assetImportParametersFileExistsForThisAsset == false)
                // {
                PathToSourceAsset = rawAssetPath
            };
            //
            //     // we save this .importParameters file as /Library/car.obj.importParameters
            //
            //     QuickSerializer.SaveFile<AssetImportParameters_Material>(importParametersFilePath,
            //         assetImportParametersMaterial);
            // }
            // else
            // {
            //     assetImportParametersMaterial =
            //         QuickSerializer.ReadFile<AssetImportParameters_Material>(importParametersFilePath);
            // }

            // AssetImportParameters[id] = assetImportParametersMaterial;


            if (canImport)
            {
                Asset_Material material = (Importers[typeof(Asset_Material)] as AssetImporter_Material)
                    .ImportAsset(assetImportParametersMaterial);

                // Assets[id] = material;
            }
        }

        if (AssetPathExtensions.IsFileTexture(rawAssetPath))
        {
            AssetImportParameters_Texture assetImportParametersTexture;
            if (assetImportParametersFileExistsForThisAsset == false)
            {
                assetImportParametersTexture = new AssetImportParameters_Texture
                {
                    PathToSourceAsset = rawAssetPath
                };

                Serializer.SaveFileJSON<AssetImportParameters_Texture>(importParametersFilePath,
                    assetImportParametersTexture);
            }
            else
            {
                assetImportParametersTexture =
                    Serializer.ReadFileJSON<AssetImportParameters_Texture>(importParametersFilePath);
            }

            AssetImportParameters[id] = assetImportParametersTexture;


            // if (canImport == false)
            // {

            Asset_Texture assetTexture =
                (Importers[typeof(Asset_Texture)] as AssetImporter_Texture).ImportAsset(
                    assetImportParametersTexture);


            Tofu.AssetLoadManager.AddAsset<Tofu3D.Asset_Texture>(assetTexture);
            // Tofu.AssetFileCache.AddAsset(assetTexture);
            // }
        }

        // Debug.Log("Asset import finished");
    }


    public void ImportAsset(string rawAssetPath, bool reimportIfExists = false)
    {
        const bool IMPORT_ON_NEW_THREAD = false;
        if (IMPORT_ON_NEW_THREAD)
        {
            Thread importThread = new Thread(() => { ImportAssetInNewThread(rawAssetPath, reimportIfExists); })
            {
                Name = "Asset import thread",
                IsBackground = true
            };
            importThread.Start();
        }
        else
        {
            ImportAssetInNewThread(rawAssetPath, reimportIfExists);
        }
    }

    public void ImportAllAssets(bool reimportIfExists = false)
    {
        List<string> allPaths = new List<string>();
        allPaths.AddRange(Directory.GetFiles(Folders.Assets, "", SearchOption.AllDirectories));
        allPaths.AddRange(Directory.GetFiles(Folders.Resources, "", SearchOption.AllDirectories));
        // scan Assets folder
        // string[] rawAssetPaths = new[] { file };

        // create AssetCreationParams<Asset_Model> for car if it doesnt exist
        foreach (string rawAssetPath in allPaths)
        {
            ImportAsset(rawAssetPath, reimportIfExists);
        }
    }

    public void ImportAllTextures(bool reimportIfExists = false)
    {
        List<string> allPaths = new List<string>();
        allPaths.AddRange(Directory.GetFiles(Folders.Assets, "", SearchOption.AllDirectories));
        allPaths.AddRange(Directory.GetFiles(Folders.Resources, "", SearchOption.AllDirectories));

        foreach (string rawAssetPath in allPaths)
        {
            if (AssetPathExtensions.IsFileTexture(rawAssetPath) == false)
            {
                continue;
            }

            ImportAsset(rawAssetPath, reimportIfExists);
        }
    }

    private bool AssetFileExists(string assetPath)
    {
        return File.Exists(assetPath);
    }

    private bool AssetImportParamsFileExists(string assetPath)
    {
        return File.Exists(AssetPathExtensions.GetPathOfImportParametersOfSourceAssetFile(assetPath));
    }
}