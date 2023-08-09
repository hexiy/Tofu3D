/*using System.IO;

namespace Tofu3D;

public static class ModelAssetManager
{
	public static Model LoadModel(string modelPath)
	{
		using (StreamReader sr = new(modelPath))
		{
			XmlSerializer xmlSerializer = new(typeof(Model));
			Model model = (Model) xmlSerializer.Deserialize(sr);
			Mesh.AssetPath = modelPath;
			/*if (mat.shader != null)
			{
				mat.SetShader(mat.shader);
			}#1#

			return model;
		}
	}

	public static void SaveModel(Model model)
	{
		using (StreamWriter sw = new(Mesh.AssetPath))
		{
			XmlSerializer xmlSerializer = new(typeof(Model));

			xmlSerializer.Serialize(sw, model);
		}
	}
}*/