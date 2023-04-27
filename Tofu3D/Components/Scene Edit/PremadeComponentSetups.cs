using System.IO;

namespace Tofu3D;

public static class PremadeComponentSetups
{
	public static ModelRenderer PrepareCube(ModelRenderer modelRenderer)
	{
		//modelRenderer.material.path
		modelRenderer.LoadTexture(Path.Combine(Folders.Textures, "solidColor.png"));
		modelRenderer.Model = AssetManager.Load<Model>(Path.Combine(Folders.Models, "cube.obj"));

		return modelRenderer;
	}
}