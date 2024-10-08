using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Scripts;
using Tofu3D;

namespace Tofu3D;

public class AssetDatabase
{
    public Dictionary<int, AssetBase> Assets { get; private set; } = new(); // int is id(path hashcode)
    // public Dictionary<int, AssetImportParametersBase> AssetImportParameters { get; private set; } = new();
    public Dictionary<Type, IAssetImporter> Importers { get; private set; } = new();

    public AssetDatabase()
    {
        RegisterAssetImporter(new AssetImporter_Model());
        RegisterAssetImporter(new AssetImporter_Texture());
        RegisterAssetImporter(new AssetImporter_Material());
    }

    private void RegisterAssetImporter(IAssetImporter assetLoader)
    {
        Importers.Add(assetLoader.GetType().BaseType.GenericTypeArguments[0], assetLoader);
    }

    public void RefreshDatabase()
    {
        string[] rawAssetPaths = Directory.GetFiles(Folders.Assets, "", SearchOption.AllDirectories);

        // scan Assets folder
        // string[] rawAssetPaths = new[] { file };

        // create AssetCreationParams<Asset_Model> for car if it doesnt exist
        foreach (string rawAssetPath in rawAssetPaths)
        {
            int id = rawAssetPath.GetHashCode();

            string rawAssetFileName = Path.GetFileName(rawAssetPath); // with extension

            string importParametersFilePath = rawAssetFileName.FromFileNameToPathToLibraryImportParameters();
            string assetFilePath = rawAssetFileName.FromRawAssetFileNameToPathOfAssetInLibrary();

            bool assetImportParametersFileExistsForThisAsset = AssetImportParamsFileExists(rawAssetFileName);


            if (rawAssetPath.Contains(".obj"))
            {
                AssetImportParameters_Model assetImportParametersModel = new AssetImportParameters_Model();
                // if (assetImportParametersFileExistsForThisAsset == false)
                // {
                assetImportParametersModel.PathToSourceAsset = rawAssetPath;
                //
                //     // we save this .importParameters file as /Library/car.obj.importParameters
                //
                //     QuickSerializer.SaveFile<AssetImportParameters_Model>(importParametersFilePath,
                //         assetImportParametersModel);
                // }
                // else
                // {
                //     assetImportParametersModel =
                //         QuickSerializer.ReadFile<AssetImportParameters_Model>(importParametersFilePath);
                // }
                //
                // AssetImportParameters[id] = assetImportParametersModel;

                bool assetExists = AssetFileExists(assetFilePath);
                if (assetExists == false)
                {
                    Asset_Model model = (Importers[typeof(Asset_Model)] as AssetImporter_Model)
                        .ImportAsset(assetImportParametersModel);

                    Assets[id] = model;
                }
            }

            if (rawAssetPath.Contains(".mat"))
            {
                AssetImportParameters_Material assetImportParametersMaterial = new AssetImportParameters_Material();
                // if (assetImportParametersFileExistsForThisAsset == false)
                // {
                assetImportParametersMaterial.PathToSourceAsset = rawAssetPath;
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

                bool assetExists = AssetFileExists(assetFilePath);
                if (assetExists == false)
                {
                    Asset_Material material = (Importers[typeof(Asset_Material)] as AssetImporter_Material)
                        .ImportAsset(assetImportParametersMaterial);

                    Assets[id] = material;
                }
            }

            if (rawAssetPath.Contains(".png") || rawAssetPath.Contains(".jpg"))
            {
                AssetImportParameters_Texture assetImportParametersTexture = new AssetImportParameters_Texture();
                // if (assetImportParametersFileExistsForThisAsset == false)
                // {
                assetImportParametersTexture.PathToSourceAsset = rawAssetPath;
                //
                //     // we save this .importParameters file as /Library/car.obj.importParameters
                //
                //     QuickSerializer.SaveFile<AssetImportParameters_Texture>(importParametersFilePath,
                //         assetImportParametersTexture);
                // }
                // else
                // {
                //     assetImportParametersTexture =
                //         QuickSerializer.ReadFile<AssetImportParameters_Texture>(importParametersFilePath);
                // }
                //
                // AssetImportParameters[id] = assetImportParametersTexture;

                bool assetExists = AssetFileExists(assetFilePath);
                // if (assetExists == false)
                // {
                Asset_Texture texture = (Importers[typeof(Asset_Texture)] as AssetImporter_Texture)
                    .ImportAsset(assetImportParametersTexture);

                Assets[id] = texture;
                // }
            }
        }
    }

    private bool AssetFileExists(string assetPath)
    {
        return File.Exists(assetPath);
    }

    private bool AssetImportParamsFileExists(string assetPath)
    {
        assetPath = assetPath.FromRawAssetFileNameToPathOfAssetInLibrary();
        return File.Exists(assetPath + ".importParameters");
    }
}