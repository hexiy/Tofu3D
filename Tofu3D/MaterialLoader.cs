﻿// using System.IO;
//
// namespace Tofu3D;
//
// public class MaterialLoader : AssetLoader<Material>
// {
//     private readonly XmlSerializer _xmlSerializer;
//
//     public MaterialLoader()
//     {
//         _xmlSerializer = new XmlSerializer(typeof(Material),
//             new[] { typeof(Texture), typeof(AssetBase), typeof(Shader) });
//     }
//
//     public override Material SaveAsset(ref Material asset, AssetLoadSettingsBase loadSettings)
//     {
//         // make sure to use loadSettings.Path not asset.path
//         StreamWriter sw = new(loadSettings.Path);
//
//         // asset.IsValid = true;
//         _xmlSerializer.Serialize(sw, asset);
//
//         sw.Close();
//
//         // asset.IsValid = false;
//
//         Tofu.AssetManager.Unload(asset, loadSettings);
//         asset = Tofu.AssetManager.Load<Asset_Material>(loadSettings);
//         return asset;
//         // return (Material) LoadAsset(loadSettings);
//     }
//
//     public override void UnloadAsset(Asset<Material> asset)
//     {
//         // GL.DeleteTexture(asset.AssetRuntimeHandle.Id); what the hell lawl
//         (asset as Material).Shader.Dispose();
//     }
//
//     public override Asset<Material> LoadAsset(AssetLoadSettingsBase assetLoadSettings)
//     {
//         var loadSettings = assetLoadSettings as MaterialLoadSettings;
//
//         if (loadSettings.Path.Contains(".mat") == false)
//         {
//             loadSettings.Path += ".mat";
//         }
//
//         loadSettings.Path = AssetUtils.ValidateAssetPath(loadSettings.Path);
//
//         if (File.Exists(loadSettings.Path) == false)
//         {
//             loadSettings.Path = Path.Combine(Folders.Materials, loadSettings.Path);
//         }
//
//
//         StreamReader sr = new(loadSettings.Path);
//
//         Material material = null;
//         try
//         {
//             material = (Material)_xmlSerializer.Deserialize(sr);
//         }
//         catch (Exception ex)
//         {
//             Debug.Log(ex.Message);
//             sr.Close();
//             return null;
//         }
//
//         sr.Close();
//         material.LoadTextures();
//         if (material.Shader != null)
//         {
//             material.InitShader();
//         }
//
//         material.InitAssetRuntimeHandle(material.Vao);
//         // material.IsValid = true;
//         return material;
//     }
// }