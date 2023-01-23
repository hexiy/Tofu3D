using System.IO;

namespace Tofu3D;

public class ModelCache
{
	static List<Model> _loadedModels = new();

	public static Model GetModel(string name)
	{
		if (name.Contains(".model") == false)
		{
			name += ".model";
		}

		for (int i = 0; i < _loadedModels.Count; i++)
		{
			if (_loadedModels[i].Path.Contains(name))
			{
				return _loadedModels[i];
			}
		}

		_loadedModels.Add(ModelAssetManager.LoadModel(Path.Combine(Folders.Models, name)));

		for (int i = 0; i < _loadedModels.Count; i++)
		{
			if (_loadedModels[i].Path.Contains(name))
			{
				return _loadedModels[i];
			}
		}

		return null;
	}
}