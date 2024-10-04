namespace Tofu3D;

public class SpriteRendererInstanced : Renderer
{
    public override void Awake()
    {
        InstancingData = new RendererInstancingData();

        // SetNativeSize += () => { UpdateBoxShapeSize(); };
        SetDefaultMaterial();
        // if (Texture == null)
        // {
        // 	Texture = new Texture();
        // }
        // else
        // {
        // 	TextureLoadSettings textureLoadSettings = TextureLoadSettings.DefaultSettingsSpritePixelArt;
        // 	Texture = AssetManager.Load<Texture>(Texture.AssetPath, textureLoadSettings);
        // }
        // if (Mesh)
        // {
        Mesh = new Mesh();
        Mesh.VertexBufferDataLength = 24;
        Mesh.VerticesCount = 6;

        BufferFactory.CreateSpriteRendererBuffer(ref Mesh.Vao);


        // Mesh = Tofu.AssetManager.Load<Mesh>(Mesh.Path);
        // }
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
        Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref InstancingData, VertexBufferStructureType.Quad,
            remove: true);

        base.OnDisabled();
    }
    /*public override void CreateMaterial()
    {
        material = new Material();
        Shader shader = new(Path.Combine(Folders.Shaders, "SpriteRenderer.glsl"));
        material.SetShader(shader);
    }*/

    // internal virtual void UpdateBoxShapeSize()
    // {
    // 	if (BoxShape != null)
    // 	{
    // 		if (Transform.IsInCanvas)
    // 		{
    // 			BoxShape.Size = Texture.Size;
    // 		}
    // 		else
    // 		{
    // 			BoxShape.Size = Texture.Size;
    // 		}
    // 	}
    // }

    public override void SetDefaultMaterial()
    {
        // if (Material?.AssetPath.Length == 0)
        // {
        // 	Material = AssetManager.Load<Material>("SpriteRenderer");
        // }

        // Material = AssetManager.Load<Material>("SpriteRendererInstanced");
        Material = Tofu.AssetManager.Load<Material>("SpriteRendererInstanced");

        // base.SetDefaultMaterial();
    }

    public override void Render()
    {
        if (GameObject.IsStatic && InstancingData.InstancingDataDirty == false &&
            InstancingData.MatrixDirty == false)
        {
            return;
        }

        if (BoxShape == null)
        {
            return;
        }

        if (Mesh == null)
        {
            return;
        }

        var updatedData =
            Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref InstancingData, VertexBufferStructureType.Quad);
        if (updatedData)
        {
            InstancingData.InstancingDataDirty = false;
        }
    }
}