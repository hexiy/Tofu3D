using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class AssetManager
{
    private AssetDatabase _assetDatabase;

    public AssetManager(AssetDatabase assetDatabase)
    {
        this._assetDatabase = assetDatabase;
    }

    public List<T> GetAllLoadedAssetsOfType<T>() where T : Asset<T>
    {
        List<T> foundAssets = new();
        var t = typeof(T);

        foreach (var keyValuePair in _assetDatabase.Assets)
        {
            if (keyValuePair.Value.Handle.AssetType == t)
            {
                foundAssets.Add(keyValuePair.Value as T);
            }
        }

        return foundAssets;
    }

    public T? Load<T>(string path) where T : Asset<T>
    {
        _assetDatabase.RefreshDatabase();

        int id = path.GetHashCode();
        bool existsInDatabase = _assetDatabase.Assets.ContainsKey(id);
        // 
        T asset = null;

        if (existsInDatabase)
        {
            asset = _assetDatabase.Assets[id] as T;
        }
        else
        {
            // asset = (_assetDatabase.Importers[typeof(T)] as AssetImporter<T>).ImportAsset() as T;
            // _assetDatabase.Assets[id] = asset;
        }

        return asset;
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