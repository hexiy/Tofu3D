using Tofu3D.Rendering;

public class ModelRendererInstanced : Renderer
{
	public Model Model;

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
		if (GameObject.IsStatic && InstancingData.InstancingDataDirty == false && InstancingData.MatrixDirty == false)
		{
			return;
		}

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


		// float speed = 2;
		// float posY = Mathf.Sin(Time.EditorElapsedTime*speed + Transform.LocalPosition.X / 10) * 3 + Mathf.Sin(Time.EditorElapsedTime*speed + Transform.LocalPosition.Z / 10) * 4;
		// Transform.LocalPosition = Transform.LocalPosition.Set(y: posY);
		bool updatedData = Tofu.I.InstancedRenderingSystem.UpdateObjectData(this);
		if (updatedData)
		{
			InstancingData.InstancingDataDirty = false;
		}
	}
}