using Tofu3D.Rendering;

public class ModelRendererInstanced : Renderer
{
	public override void Awake()
	{
		InstancingData = new RendererInstancingData();
		base.Awake();
	}

	public override void OnEnabled()
	{
		InstancingData.InstancingDataDirty = true;
		InstancingData.MatrixDirty = true;

		base.OnEnabled();
	}

	public override void OnDisabled()
	{
		Tofu.I.InstancedRenderingSystem.UpdateObjectData(this, remove: true);

		base.OnDisabled();
	}

	public override void SetDefaultMaterial()
	{
		if (Material?.Path.Length == 0 || Material == null)
		{
			Material = Tofu.I.AssetManager.Load<Material>("ModelRendererInstanced");
		}
		else
		{
			Material = Tofu.I.AssetManager.Load<Material>(Material.Path);
		}

		if (Mesh?.Path.Length>0)
		{
			Mesh = Tofu.I.AssetManager.Load<Mesh>(Mesh.Path);
		}
	}

	public override void Render()
	{
		if (GameObject.IsStatic && InstancingData.InstancingDataDirty == false && InstancingData.MatrixDirty == false)
		{
			return;
		}

		if (Mesh == null)
		{
			return;
		}

		/*
		 bool isTransformHandle = GameObject == TransformHandle.I.GameObject;
		if (isTransformHandle && (Tofu.I.RenderPassSystem.CurrentRenderPassType != RenderPassType.Opaques && Tofu.I.RenderPassSystem.CurrentRenderPassType != RenderPassType.UI))
		{
			return;
		}

		if (Transform.IsInCanvas && Tofu.I.RenderPassSystem.CurrentRenderPassType != RenderPassType.UI || Transform.IsInCanvas == false && Tofu.I.RenderPassSystem.CurrentRenderPassType == RenderPassType.UI)
		{
			return;
		}

		if (Model == null)
		{
			return;
		}*/

		
		bool updatedData = Tofu.I.InstancedRenderingSystem.UpdateObjectData(this);
		if (updatedData)
		{
			InstancingData.InstancingDataDirty = false;
		}
	}
}