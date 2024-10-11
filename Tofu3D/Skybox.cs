using System.IO;

namespace Tofu3D;

[ExecuteInEditMode]
public class Skybox : Component, IComponentUpdateable
{
    private Asset_Material _material;
    private RuntimeCubemapTexture _texture;
    public float Fov = 60;

    public void Update()
    {
        // Debug.StatSetValue("SkyboxList Textures", $"{Textures.Count}");
        // Debug.StatSetValue("SkyboxList Ints", $"SkyboxList Ints count {Ints.Count}");
        // Debug.StatSetValue("SkyboxList Colors", $"{Colors.Count}");
    }

    public override void Awake()
    {
        _material = Tofu.AssetLoadManager.Load<Asset_Material>("/Assets/Materials/Skybox.mat");

        _texture = new RuntimeCubemapTexture();
        string[] texturePaths =
        {
            Path.Combine(Folders.TexturesInAssets, "skybox2", "Daylight Box_Right.bmp"),
            Path.Combine(Folders.TexturesInAssets, "skybox2", "Daylight Box_Left.bmp"),
            Path.Combine(Folders.TexturesInAssets, "skybox2", "Daylight Box_Top.bmp"),
            Path.Combine(Folders.TexturesInAssets, "skybox2", "Daylight Box_Bottom.bmp"),
            Path.Combine(Folders.TexturesInAssets, "skybox2", "Daylight Box_Front.bmp"),
            Path.Combine(Folders.TexturesInAssets, "skybox2", "Daylight Box_Back.bmp")
        };

        AssetLoadParameters_CubemapTexture loadParameters = new() { PathsToSourceTextures = texturePaths };
        _texture = Tofu.AssetLoadManager.Load<RuntimeCubemapTexture>(texturePaths[0],
            loadParameters); // texturePaths[0] because for now every Load call will have path

        base.Awake();
    }

    public override void OnEnabled()
    {
        Tofu.RenderPassSystem.RegisterRender(RenderPassType.Skybox, RenderSkybox);

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        Tofu.RenderPassSystem.RemoveRender(RenderPassType.Skybox, RenderSkybox);

        base.OnDisabled();
    }

    private void RenderSkybox()
    {
        if (Enabled == false || GameObject.ActiveInHierarchy == false)
        {
            return;
        }


        var forwardLocal = Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 0, 1));
        var upLocal = Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 1, 0));

        var viewMatrix = Matrix4x4.CreateLookAt(Vector3.Zero, forwardLocal, upLocal) *
                         Matrix4x4.CreateScale(-1, 1, 1);

        Fov = Mathf.Clamp(Fov, 0.000001f, 179);
        var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov),
            Camera.MainCamera.Size.X / Camera.MainCamera.Size.Y, 0.01f, 1);


        GL.DepthMask(false);
        Tofu.ShaderManager.UseShader(_material.Shader);

        _material.Shader.SetMatrix4X4("u_view", viewMatrix);
        _material.Shader.SetMatrix4X4("u_projection", projectionMatrix);

        Tofu.ShaderManager.BindVertexArray(_material.Vao);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(_texture.TextureId, TextureType.Cubemap);

        GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

        DebugHelper.LogDrawCall();
        GL.DepthMask(true);
    }
}