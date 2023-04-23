using System.IO;
using System.Linq;
using System.Numerics;
using Tofu3D.Rendering;

namespace Tofu3D;

public class SceneSkyboxRenderer
{
	Material _material;
	Texture _texture;

	Scene _scene;

	public static SceneSkyboxInspectable Inspectable { get; private set; }

	public SceneSkyboxRenderer(Scene scene)
	{
		Inspectable = new SceneSkyboxInspectable();
		_scene = _scene;

		_material = AssetManager.Load<Material>("Skybox");

		_texture = new Texture();
		string[] texturePaths = new[]
		                        {
			                        /*Path.Combine(Folders.Textures, "skyCubemap", "sky_left.png"),
			                        Path.Combine(Folders.Textures, "skyCubemap", "sky_right.png"),
			                        Path.Combine(Folders.Textures, "skyCubemap", "sky_down.png"),
			                        Path.Combine(Folders.Textures, "skyCubemap", "sky_up.png"),
			                        Path.Combine(Folders.Textures, "skyCubemap", "sky_front.png"),
			                        Path.Combine(Folders.Textures, "skyCubemap", "sky_back.png"),*/
			                        Path.Combine(Folders.Textures, "skybox2", "Daylight Box_Right.bmp"),
			                        Path.Combine(Folders.Textures, "skybox2", "Daylight Box_Left.bmp"),
			                        Path.Combine(Folders.Textures, "skybox2", "Daylight Box_Bottom.bmp"),
			                        Path.Combine(Folders.Textures, "skybox2", "Daylight Box_Top.bmp"),
			                        Path.Combine(Folders.Textures, "skybox2", "Daylight Box_Front.bmp"),
			                        Path.Combine(Folders.Textures, "skybox2", "Daylight Box_Back.bmp"),
		                        };

		string oneStringForAllSixTextures = String.Concat(texturePaths);
		TextureLoadSettings textureLoadSettings = new TextureLoadSettings(path: oneStringForAllSixTextures,
		                                                                  textureType: TextureType.Cubemap);
		_texture = AssetManager.Load<Texture>(textureLoadSettings);

		// _texture.Load(textureLoadSettings);

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

		ShaderCache.UseShader(_material.Shader);


		Vector3 forwardLocal = Camera.I.Transform.TransformDirectionToWorldSpace(new Vector3(0, 0, 1));
		Vector3 upLocal = Camera.I.Transform.TransformDirectionToWorldSpace(new Vector3(0, 1, 0));

		Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(cameraPosition: Vector3.Zero, cameraTarget: forwardLocal, cameraUpVector: upLocal);

		Inspectable.Fov = Mathf.Clamp(Inspectable.Fov, 0.000001f, 179);
		Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Inspectable.Fov), Camera.I.Size.X / Camera.I.Size.Y, 0.01f, 1);

		_material.Shader.SetMatrix4X4("u_view", viewMatrix);
		_material.Shader.SetMatrix4X4("u_projection", projectionMatrix);

		ShaderCache.BindVertexArray(_material.Vao);

		GL.ActiveTexture(TextureUnit.Texture0);
		TextureHelper.BindTexture(_texture.TextureId, TextureType.Cubemap);

		if (KeyboardInput.WasKeyJustPressed(Keys.D1))
		{
			Inspectable.Fov -= Time.EditorDeltaTime;
		}

		if (KeyboardInput.WasKeyJustPressed(Keys.D2))
		{
			Inspectable.Fov += Time.EditorDeltaTime;
		}

		GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

		DebugHelper.LogDrawCall();
		GL.DepthMask(true);
	}
}