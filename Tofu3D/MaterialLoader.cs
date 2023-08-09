using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Tofu3D;

public class MaterialLoader : AssetLoader<Material>
{
    XmlSerializer _xmlSerializer;

    public MaterialLoader()
    {
        _xmlSerializer = new(typeof(Material), new Type[] { typeof(Texture), typeof(AssetBase), typeof(Shader) });
    }

    public override Material SaveAsset(ref Material asset, AssetLoadSettingsBase loadSettings)
    {
        StreamWriter sw = new(asset.Path);

        // asset.IsValid = true;
        _xmlSerializer.Serialize(sw, asset);

        sw.Close();

        // asset.IsValid = false;

        Tofu.I.AssetManager.Unload(asset, loadSettings);
        asset = Tofu.I.AssetManager.Load<Material>(loadSettings);
        return asset;
        // return (Material) LoadAsset(loadSettings);
    }

    public override void UnloadAsset(Asset<Material> asset)
    {
        // GL.DeleteTexture(asset.AssetRuntimeHandle.Id); what the hell lawl
        (asset as Material).Shader.Dispose();
    }

    public override Asset<Material> LoadAsset(AssetLoadSettingsBase assetLoadSettings)
    {
        MaterialLoadSettings loadSettings = assetLoadSettings as MaterialLoadSettings;

        if (loadSettings.Path.Contains(".mat") == false)
        {
            loadSettings.Path += ".mat";
        }

        loadSettings.Path = AssetUtils.ValidateAssetPath(loadSettings.Path);

        if (File.Exists(loadSettings.Path) == false)
        {
            loadSettings.Path = Path.Combine(Folders.Materials, loadSettings.Path);
        }


        StreamReader sr = new(loadSettings.Path);

        Material material = null;
        try
        {
            material = (Material)_xmlSerializer.Deserialize(sr);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            sr.Close();
            return null;
        }

        sr.Close();
        material.LoadTextures();
        if (material.Shader != null)
        {
            material.InitShader();
        }

        material.InitAssetRuntimeHandle(material.Vao);
        // material.IsValid = true;
        return material;
    }
}