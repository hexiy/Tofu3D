using Tofu3D.Rendering;

public class ModelRendererInstanced : TextureRenderer
{
	public Model Model;

	public override void Awake()
	{
		if (Texture == null)
		{
			Texture = new Texture();
		}
		else
		{
			LoadTexture(Texture.AssetPath);
		}

		base.Awake();
	}

	public override void SetDefaultMaterial()
	{
		if (Material?.AssetPath.Length == 0 || Material == null)
		{
			Material = AssetManager.Load<Material>("ModelRendererInstanced");
		}
		else
		{
			Material = AssetManager.Load<Material>(Material.AssetPath);
		}

		if (Model)
		{
			Model = AssetManager.Load<Model>(Model.AssetPath);
		}
	}

	public override void Render()
	{
		bool isTransformHandle = GameObject == TransformHandle.I.GameObject;
		if (isTransformHandle && (RenderPassSystem.CurrentRenderPassType != RenderPassType.Opaques && RenderPassSystem.CurrentRenderPassType != RenderPassType.UI))
		{
			return;
		}

		if (Transform.IsInCanvas && RenderPassSystem.CurrentRenderPassType != RenderPassType.UI || Transform.IsInCanvas == false && RenderPassSystem.CurrentRenderPassType == RenderPassType.UI)
		{
			return;
		}

		if (Model == null)
		{
			return;
		}


		if (GameObject == TransformHandle.I?.GameObject)
		{
			GL.Disable(EnableCap.DepthTest);
		}
		else
		{
			GL.Enable(EnableCap.DepthTest);
		}

		Tofu.I.InstancedRenderingSystem.UpdateObjectData(Model, Material, this);
	}
}