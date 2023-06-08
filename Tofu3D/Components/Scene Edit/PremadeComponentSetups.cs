using System.IO;

namespace Tofu3D;

public static class PremadeComponentSetups
{
	public static ModelRendererInstanced PrepareCube(ModelRendererInstanced modelRenderer)
	{
		//modelRenderer.material.path
		// modelRenderer.Material= AssetManager.Load<Material>("ModelSolid");adasdadasd
		// modelRenderer.Material.AlbedoTexture = AssetManager.Load<Texture>(Path.Combine(Folders.Textures, "solidColor.png"));
		modelRenderer.Mesh = AssetManager.Load<Mesh>(Path.Combine(Folders.Models, "cube.obj"));

		return modelRenderer;
	}
}