using System.IO;

namespace Tofu3D.Components.Renderers;

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
			if (_loadedMaterials[i].Path.Contains(name))
			{
				return _loadedMaterials[i];
			}
		}

		_loadedMaterials.Add(MaterialAssetManager.LoadMaterial(Path.Combine(Folders.Materials, name)));

		for (int i = 0; i < _loadedMaterials.Count; i++)
		{
			if (_loadedMaterials[i].Path.Contains(name))
			{
				return _loadedMaterials[i];
			}
		}

		return null;
	}
}