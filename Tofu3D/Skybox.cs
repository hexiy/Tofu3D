﻿using System.IO;

namespace Tofu3D;

[ExecuteInEditMode]
public class Skybox : Component, IComponentUpdateable
{
    private Material _material;
    private CubemapTexture _texture;
    public float Fov = 60;

    public override void Awake()
    {
        _material = Tofu.AssetManager.Load<Material>("Skybox");

        _texture = new CubemapTexture();
        string[] texturePaths = new[]
        {
            Path.Combine(Folders.Textures, "skybox2", "Daylight Box_Right.bmp"),
            Path.Combine(Folders.Textures, "skybox2", "Daylight Box_Left.bmp"),
            Path.Combine(Folders.Textures, "skybox2", "Daylight Box_Top.bmp"),
            Path.Combine(Folders.Textures, "skybox2", "Daylight Box_Bottom.bmp"),
            Path.Combine(Folders.Textures, "skybox2", "Daylight Box_Front.bmp"),
            Path.Combine(Folders.Textures, "skybox2", "Daylight Box_Back.bmp")
        };

        CubemapTextureLoadSettings cubemapTextureLoadSettings = new(texturePaths);
        _texture = Tofu.AssetManager.Load<CubemapTexture>(cubemapTextureLoadSettings);

        base.Awake();
    }

    public void Update()
    {
        // Debug.StatSetValue("SkyboxList Textures", $"{Textures.Count}");
        // Debug.StatSetValue("SkyboxList Ints", $"SkyboxList Ints count {Ints.Count}");
        // Debug.StatSetValue("SkyboxList Colors", $"{Colors.Count}");
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
        if (Enabled == false || GameObject.ActiveInHierarchy == false) return;

        GL.DepthMask(false);

        Tofu.ShaderManager.UseShader(_material.Shader);

        Vector3 forwardLocal = Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 0, 1));
        Vector3 upLocal = Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(new Vector3(0, 1, 0));

        Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(Vector3.Zero, forwardLocal, upLocal) *
                               Matrix4x4.CreateScale(-1, 1, 1);

        Fov = Mathf.Clamp(Fov, 0.000001f, 179);
        Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov),
            Camera.MainCamera.Size.X / Camera.MainCamera.Size.Y, 0.01f, 1);

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