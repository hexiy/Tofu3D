using System.IO;

namespace Tofu3D;

public static class PremadeComponentSetups
{
	public static ModelRenderer PrepareCube(ModelRenderer modelRenderer)
	{
		//modelRenderer.material.path
		modelRenderer.LoadTexture(Path.Combine(Folders.Textures, "solidColor.png"));
		return modelRenderer;
	}
}