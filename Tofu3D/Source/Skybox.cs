namespace TofuEngine;

[ExecuteInEditMode]
public class Skybox : Component, IComponentUpdateable, IHasMaterial
{
    private Asset_Material _material;
    public Asset_Material GetMaterial => _material;

    private RuntimeCubemapTexture _texture;
    public float Fov = 60;

    public RuntimeCubemapTexture GetCubemapTexture() => _texture;

    public void Update()
    {
        // Debug.StatSetValue("SkyboxList Textures", $"{Textures.Count}");
        // Debug.StatSetValue("SkyboxList Ints", $"SkyboxList Ints count {Ints.Count}");
        // Debug.StatSetValue("SkyboxList Colors", $"{Colors.Count}");
    }

    public override void Awake()
    {
        // _material = Tofu.AssetLoadManager.Load<Asset_Material>("/Assets/Materials/Skybox.mat");
        _material = new Asset_Material()
            { Shader = Tofu.ShaderManager.LoadShader(TofuPath.Combine(Folders.ShadersInAssets, "Skybox.glsl")) };
        _material.LoadShader();
        _texture = new RuntimeCubemapTexture();
        string[] texturePaths =
        {
            TofuPath.Combine(Folders.TexturesInAssets, "skybox2", "Daylight Box_Right.bmp"),
            TofuPath.Combine(Folders.TexturesInAssets, "skybox2", "Daylight Box_Left.bmp"),
            TofuPath.Combine(Folders.TexturesInAssets, "skybox2", "Daylight Box_Top.bmp"),
            TofuPath.Combine(Folders.TexturesInAssets, "skybox2", "Daylight Box_Bottom.bmp"),
            TofuPath.Combine(Folders.TexturesInAssets, "skybox2", "Daylight Box_Front.bmp"),
            TofuPath.Combine(Folders.TexturesInAssets, "skybox2", "Daylight Box_Back.bmp")
        };

        AssetLoadParameters_CubemapTexture loadParameters = new AssetLoadParameters_CubemapTexture
            { PathsToSourceTextures = texturePaths };
        _texture = Tofu.AssetLoadManager.Get<RuntimeCubemapTexture>(texturePaths[0]+"cubemap",
            loadParameters); // texturePaths[0] because for now every Load call will have path

        base.Awake();
    }

    public void RenderSkybox()
    {
        if (EnabledSelf == false || GameObject.ActiveInHierarchy == false)
        {
            return;
        }


        Vector3 forwardLocal = Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 0, 1));
        Vector3 upLocal = Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 1, 0));

        Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(Vector3.Zero, forwardLocal, upLocal) *
                               Matrix4x4.CreateScale(-1, 1, 1);

        Fov = Mathf.Clamp(Fov, 0.000001f, 179);
        Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
            OpenTK.Mathematics.MathHelper.DegreesToRadians(Fov),
            Camera.MainCamera.Size.X / Camera.MainCamera.Size.Y, 0.01f, 1);


        // GL.DepthMask(false);
        Tofu.ShaderManager.UseShader(_material.Shader);

        _material.Shader.SetMatrix4X4("u_view", viewMatrix);
        _material.Shader.SetMatrix4X4("u_projection", projectionMatrix);

        Tofu.ShaderManager.BindVertexArray(Tofu.BasicMeshesCollection.CubemapMesh.Vao);

        GL.ActiveTexture(TextureUnit.Texture0);
        TextureHelper.BindTexture(_texture.TextureId, TextureType.Cubemap);
        GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

        DebugHelper.LogDrawCall();
        // GL.DepthMask(true);
    }
}