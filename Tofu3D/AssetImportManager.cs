using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Scripts;
using Tofu3D;

namespace Tofu3D;

// Transforms .obj,.png files into .asset files in /Library/
public class AssetImportManager
{
    // public Dictionary<int, AssetBase> Assets { get; private set; } = new(); // int is id(path hashcode)
    // public Dictionary<int, AssetImportParametersBase> AssetImportParameters { get; private set; } = new();
    public Dictionary<Type, IAssetImporter> Importers { get; private set; } = new();

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

    public void ImportAllAssets()
    {
        List<string> allPaths = new List<string>();
        allPaths.AddRange(Directory.GetFiles(Folders.Assets, "", SearchOption.AllDirectories));
        allPaths.AddRange(Directory.GetFiles(Folders.Resources, "", SearchOption.AllDirectories));
        // scan Assets folder
        // string[] rawAssetPaths = new[] { file };

        // create AssetCreationParams<Asset_Model> for car if it doesnt exist
        foreach (string rawAssetPath in allPaths)
        {
            int id = rawAssetPath.GetHashCode();

            string rawAssetFileName = Path.GetFileName(rawAssetPath); // with extension

            string importParametersFilePath = rawAssetFileName.FromFileNameToPathToLibraryImportParameters();
            string assetFilePath = rawAssetFileName.FromRawAssetFileNameToPathOfAssetInLibrary();
            bool assetExists = AssetFileExists(assetFilePath.FromRawAssetFileNameToPathOfAssetInLibrary());
            if (assetExists)
            {
                continue;
            
            }
            bool assetImportParametersFileExistsForThisAsset = AssetImportParamsFileExists(rawAssetFileName);


            if (AssetFileExtensions.IsFileModel(rawAssetPath))
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

                if (assetExists == false)
                {
                    Asset_Model model = (Importers[typeof(Asset_Model)] as AssetImporter_Model)
                        .ImportAsset(assetImportParametersModel);

                    // Assets[id] = model;
                }
            }

            if (AssetFileExtensions.IsFileMaterial(rawAssetPath))
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


                if (assetExists == false)
                {
                    Asset_Material material = (Importers[typeof(Asset_Material)] as AssetImporter_Material)
                        .ImportAsset(assetImportParametersMaterial);

                    // Assets[id] = material;
                }
            }

            if (AssetFileExtensions.IsFileTexture(rawAssetPath))
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


                // if (assetExists == false)
                // {

                Asset_Texture assetTexture =
                    (Importers[typeof(Asset_Texture)] as AssetImporter_Texture).ImportAsset(
                        assetImportParametersTexture);


                // Assets[id] = texture;
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