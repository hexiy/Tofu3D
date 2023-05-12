using Tofu3D.Rendering;

public class ModelRendererInstanced : TextureRenderer
{
	public Model Model;
	bool _hasUpdatedInstanceBufferData;

	public override void Awake()
	{
		// _hasUpdatedInstanceBufferData = false;
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

	public override void OnEnable()
	{
		_hasUpdatedInstanceBufferData = false;
		base.OnEnable();
	}

	public override void OnDisable()
	{
		Tofu.I.InstancedRenderingSystem.UpdateObjectData(this, remove: true);

		base.OnDisable();
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
		if (GameObject.IsStatic && _hasUpdatedInstanceBufferData)
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
		Tofu.I.InstancedRenderingSystem.UpdateObjectData(this);
		_hasUpdatedInstanceBufferData = true;
	}
}