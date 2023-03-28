using System.IO;
using System.Numerics;
using Tofu3D.Rendering;

namespace Tofu3D;

public class SceneSkyboxRenderer
{
	Material _material;
	Texture _texture;

	Scene _scene;

	public SceneSkyboxRenderer(Scene scene)
	{
		_scene = _scene;

		_material = MaterialCache.GetMaterial("Skybox");

		_texture = new Texture();
		_texture.LoadCubemap(Path.Combine(Folders.Textures, "sdf.png"));

		RenderPassSystem.RegisterRender(RenderPassType.Skybox, RenderSkybox);
		Scene.SceneDisposed += OnSceneDisposed;
	}

	private void OnSceneDisposed()
	{
		RenderPassSystem.RemoveRender(RenderPassType.Skybox, RenderSkybox);
	}

	private void RenderSkybox()
	{
		
		GL.DepthMask(false);
		// GL.ClearColor(System.Drawing.Color.RosyBrown);

		// GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


		ShaderCache.UseShader(_material.Shader);


		Matrix4x4 viewMatrix = Camera.I.ViewMatrix;
		viewMatrix.M41 = 0;
		viewMatrix.M42 = 0;
		viewMatrix.M43 = 0;

		_material.Shader.SetMatrix4X4("u_view", viewMatrix);
		_material.Shader.SetMatrix4X4("u_projection", Camera.I.ProjectionMatrix);

		ShaderCache.BindVertexArray(_material.Vao);
		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		ImGuiController.CheckGlError("1");
		GL.ActiveTexture(TextureUnit.Texture0);
		TextureCache.BindTexture(_texture.Id, TextureTarget.TextureCubeMap);
		ImGuiController.CheckGlError("2");

		GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
		ImGuiController.CheckGlError("3");

		Debug.StatAddValue("Draw Calls", 1);
		GL.DepthMask(true);
	}
}