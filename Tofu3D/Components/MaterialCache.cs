using System.IO;

namespace Tofu3D;

public class MaterialCache
{
	static List<Material> _loadedMaterials = new();

	public static Material GetMaterial(string name)
	{
		if (name.Contains(".mat") == false)
		{
			name += ".mat";
		}

		for (int i = 0; i < _loadedMaterials.Count; i++)
		{
			string fileName = Path.GetFileName(_loadedMaterials[i].Path);
			if (fileName == name)
			{
				return _loadedMaterials[i];
			}
		}

		_loadedMaterials.Add(MaterialAssetManager.LoadMaterial(Path.Combine(Folders.Materials, name)));

		for (int i = 0; i < _loadedMaterials.Count; i++)
		{
			string fileName = Path.GetFileName(_loadedMaterials[i].Path);
			if (fileName == name)
			{
				return _loadedMaterials[i];
			}
		}

		return null;
	}
}