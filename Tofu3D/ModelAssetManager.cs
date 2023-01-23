using System.IO;

namespace Tofu3D;

public static class ModelAssetManager
{
	public static Model LoadModel(string modelPath)
	{
		using (StreamReader sr = new(modelPath))
		{
			XmlSerializer xmlSerializer = new(typeof(Model));
			Model model = (Model) xmlSerializer.Deserialize(sr);
			model.Path = modelPath;
			/*if (mat.shader != null)
			{
				mat.SetShader(mat.shader);
			}*/

			return model;
		}
	}

	public static void SaveModel(Model model)
	{
		using (StreamWriter sw = new(model.Path))
		{
			XmlSerializer xmlSerializer = new(typeof(Model));

			xmlSerializer.Serialize(sw, model);
		}
	}
}