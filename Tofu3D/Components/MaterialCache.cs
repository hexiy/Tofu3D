/*using System.IO;

namespace Tofu3D;

public class MaterialCache
{
	static List<Material> _loadedMaterials = new();

	public static Material ModelRendererMaterial { get; private set; }
	public static Material ModelRendererUnlitMaterial { get; private set; }

	public static Material GetMaterial(string name)
	{
		if (name.Contains(".mat") == false)
		{
			name += ".mat";
		}

		for (int i = 0; i < _loadedMaterials.Count; i++)
		{
			if (_loadedMaterials[i].FileName == name)
			{
				return _loadedMaterials[i];
			}
		}


		Material material = MaterialTofu.I.AssetManager.LoadMaterial(Path.Combine(Folders.Materials, name));


		_loadedMaterials.Add(material);
		if (ModelRendererMaterial == null)
		{
			CacheModelRendererMaterials();
		}

		return material;
	}

	private static void CacheModelRendererMaterials()
	{
		ModelRendererMaterial = GetMaterial("ModelRenderer");
		ModelRendererUnlitMaterial = GetMaterial("ModelRendererUnlit");
	}
}*/