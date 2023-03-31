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

	float rotation = 0;

	private void RenderSkybox()
	{
		GL.DepthMask(false);
		// GL.ClearColor(System.Drawing.Color.RosyBrown);

		// GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


		ShaderCache.UseShader(_material.Shader);


		// Matrix4x4 viewMatrix = Camera.I.ViewMatrix;
		Vector3 forwardLocal = Camera.I.Transform.TransformDirectionToWorldSpace(new Vector3(0, 0, 1));
		Vector3 upLocal = Camera.I.Transform.TransformDirectionToWorldSpace(new Vector3(0, 1, 0));

		Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(cameraPosition: Vector3.Zero, cameraTarget: forwardLocal, cameraUpVector: upLocal);

		Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.I.FieldOfView), Camera.I.Size.X / Camera.I.Size.Y, 0.01f, 1);

		_material.Shader.SetMatrix4X4("u_view", viewMatrix);
		_material.Shader.SetMatrix4X4("u_projection", projectionMatrix);

		ShaderCache.BindVertexArray(_material.Vao);
		// GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		GL.ActiveTexture(TextureUnit.Texture0);
		TextureCache.BindTexture(_texture.Id, TextureTarget.TextureCubeMap);

		if (KeyboardInput.WasKeyJustPressed(Keys.D1))
		{
			rotation -= Time.EditorDeltaTime;
		}

		if (KeyboardInput.WasKeyJustPressed(Keys.D2))
		{
			rotation += Time.EditorDeltaTime;
		}

		GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

		Debug.StatAddValue("Draw Calls", 1);
		GL.DepthMask(true);
	}
}