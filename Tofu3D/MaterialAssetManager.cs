using System.IO;

namespace Tofu3D;

public static class MaterialAssetManager
{
	public static void CreateDefaultMaterials()
	{
		{
			Material boxMaterial = new();

			Shader boxShader = new(Path.Combine(Folders.Shaders, "BoxRenderer.glsl"));
			boxMaterial.SetShader(boxShader);
			using (StreamWriter sw = new(Path.Combine(Folders.Materials, "BoxMaterial.mat")))
			{
				XmlSerializer xmlSerializer = new(typeof(Material));

				xmlSerializer.Serialize(sw, boxMaterial);
			}
		}
		{
			Material renderTextureMaterial = new();

			Shader renderTextureShader = new(Path.Combine(Folders.Shaders, "RenderTexture.glsl"));
			renderTextureMaterial.SetShader(renderTextureShader);
			using (StreamWriter sw = new(Path.Combine(Folders.Materials, "RenderTexture.mat")))
			{
				XmlSerializer xmlSerializer = new(typeof(Material));

				xmlSerializer.Serialize(sw, renderTextureMaterial);
			}
		}
		{
			Material renderTextureMaterial = new();

			Shader renderTextureShader = new(Path.Combine(Folders.Shaders, "SpriteRenderer.glsl"));
			renderTextureMaterial.SetShader(renderTextureShader);
			using (StreamWriter sw = new(Path.Combine(Folders.Materials, "SpriteRenderer.mat")))
			{
				XmlSerializer xmlSerializer = new(typeof(Material));

				xmlSerializer.Serialize(sw, renderTextureMaterial);
			}
		}
	}

	public static Material LoadMaterial(string materialPath)
	{
		using (StreamReader sr = new(materialPath))
		{
			XmlSerializer xmlSerializer = new(typeof(Material));
			Material mat = (Material) xmlSerializer.Deserialize(sr);
			mat.Path = materialPath;
			if (mat.Shader != null)
			{
				mat.SetShader(mat.Shader);
			}

			return mat;
		}
	}

	public static void SaveMaterial(Material material)
	{
		using (StreamWriter sw = new(material.Path))
		{
			XmlSerializer xmlSerializer = new(typeof(Material));

			xmlSerializer.Serialize(sw, material);
		}
	}
}