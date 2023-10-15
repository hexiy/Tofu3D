using System.IO;

namespace Tofu3D;

public static class PremadeComponentSetupsHelper
{
	public static ModelRendererInstanced PrepareCube(ModelRendererInstanced modelRenderer)
	{
		//modelRenderer.material.path
		// modelRenderer.Material= Tofu.AssetManager.Load<Material>("ModelSolid");adasdadasd
		// modelRenderer.Material.AlbedoTexture = Tofu.AssetManager.Load<Texture>(Path.Combine(Folders.Textures, "solidColor.png"));
		modelRenderer.Mesh = Tofu.AssetManager.Load<Mesh>(Path.Combine(Folders.Models, "cube.obj"));

		return modelRenderer;
	}
}