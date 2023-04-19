using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Tofu3D;

public class MaterialLoader : AssetLoader<Material>
{
	public override void SaveAsset(Material asset, AssetLoadSettingsBase loadSettings)
	{
		StreamWriter sw = new(asset.AssetPath);
		XmlSerializer xmlSerializer = new(typeof(Material));

		xmlSerializer.Serialize(sw, asset);

		sw.Close();
	}

	public override void UnloadAsset(Asset<Material> asset)
	{
		GL.DeleteTexture(asset.AssetRuntimeHandle.Id);
	}

	public override Asset<Material> LoadAsset(AssetLoadSettingsBase assetLoadSettings)
	{
		MaterialLoadSettings loadSettings = assetLoadSettings as MaterialLoadSettings;

		if (loadSettings.Path.Contains(".mat") == false)
		{
			loadSettings.Path += ".mat";
		}

		if (File.Exists(loadSettings.Path) == false)
		{
			loadSettings.Path = Path.Combine(Folders.Materials, loadSettings.Path);
		}


		StreamReader sr = new(loadSettings.Path);

		XmlSerializer xmlSerializer = new(typeof(Material));
		Material material = (Material) xmlSerializer.Deserialize(sr);

		if (material.Shader != null)
		{
			material.SetShader(material.Shader);
		}

		material.InitAssetRuntimeHandle(material.Vao);

		return material;
	}
}