using System.IO;

namespace Tofu3D;

public class SpriteRenderer : TextureRenderer
{
	[Hide] public virtual bool Batched { get; set; } = false;

	public override void Awake()
	{
		SetDefaultTexture(Path.Combine(Folders.Textures, "solidColor.png"));

		SetNativeSize += () => { UpdateBoxShapeSize(); };
		SetDefaultMaterial();
		if (Texture == null)
		{
			Texture = new Texture();
		}
		else
		{
			TextureLoadSettings textureLoadSettings = TextureLoadSettings.DefaultSettingsSpritePixelArt;
			Texture = AssetManager.Load<Texture>(Texture.AssetPath, textureLoadSettings);
		}

		//BatchingManager.AddObjectToBatcher(Texture.Id, this);
		base.Awake();
	}

	/*public override void CreateMaterial()
	{
		material = new Material();
		Shader shader = new(Path.Combine(Folders.Shaders, "SpriteRenderer.glsl"));
		material.SetShader(shader);
	}*/

	internal virtual void UpdateBoxShapeSize()
	{
		if (BoxShape != null)
		{
			if (IsInCanvas)
			{
				BoxShape.Size = Texture.Size;
			}
			else
			{
				BoxShape.Size = Texture.Size / Units.OneWorldUnit;
			}
		}
	}

	public override void SetDefaultMaterial()
	{
		if (Material?.FileName == null)
		{
			Material = MaterialCache.GetMaterial("SpriteRenderer");
		}

		// base.SetDefaultMaterial();
	}

	public override void OnDestroyed()
	{
		//BatchingManager.RemoveAttribs(texture.id, gameObjectID);
		base.OnDestroyed();
	}

	public override void Render()
	{
		if (OnScreen == false)
		{
			return;
		}

		if (BoxShape == null)
		{
			return;
		}

		if (Texture.Loaded == false)
		{
			return;
		}

		if (false)
		{
			// BatchingManager.UpdateAttribs(Texture.Id, GameObjectId, Transform.WorldPosition,
			//                               new Vector2(GetComponent<BoxShape>().Size.X * Transform.WorldScale.X,
			//                                           GetComponent<BoxShape>().Size.Y * Transform.WorldScale.Y),
			//                               Transform.Rotation.Z, Color);
			return;
		}

		ShaderCache.UseShader(Material.Shader);
		Material.Shader.SetVector2("u_resolution", Texture.Size);
		if (IsInCanvas)
		{
			// Material.Shader.SetMatrix4X4("u_mvp", GetModelMatrix() * Matrix4x4.CreateScale(1f/Units.OneWorldUnit));
			Material.Shader.SetMatrix4X4("u_mvp", GetModelMatrixForCanvasObject()); // * Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);
		}
		else
		{
			Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
		}

		Material.Shader.SetColor("u_rendererColor", Color);
		Material.Shader.SetVector2("u_tiling", Tiling);
		Material.Shader.SetVector2("u_offset", Offset);

		ShaderCache.BindVertexArray(Material.Vao);

		if (Material.Additive)
		{
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
		}
		else
		{
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		}

		GL.ActiveTexture(TextureUnit.Texture0);
		TextureCache.BindTexture(Texture.TextureId);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		Debug.StatAddValue("Draw Calls", 1);
	}
}

// STENCIL working

/*
public override void Render()
		{
			if (boxShape == null) return;
			if (texture.loaded == false) return;
			// stencil experiment
			GL.Enable(EnableCap.StencilTest);
			GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
			GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
			GL.StencilMask(0xFF);
			shader.Use();
			shader.SetMatrix4x4("u_mvp", GetModelViewProjection());
			shader.SetVector4("u_color", color.ToVector4());

			GL.BindVertexArray(vao);
			GL.Enable(EnableCap.Blend);

			if (additive)
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
			}
			else
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			}
			texture.Use();
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
			// stencil after
			GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
			GL.StencilMask(0x00);
			GL.Disable(EnableCap.DepthTest);

			shader.Use();
			shader.SetMatrix4x4("u_mvp", GetModelViewProjectionForOutline(thickness));
			//shader.SetVector4("u_color", new Vector4(MathF.Abs(MathF.Sin(Time.elapsedTime * 0.3f)), MathF.Abs(MathF.Cos(Time.elapsedTime * 0.3f)), 1, 1));
			shader.SetVector4("u_color", Color.Black.ToVector4());

			texture.Use();
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

			GL.StencilMask(0xFF);
			GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
			GL.Enable(EnableCap.DepthTest);

			GL.BindVertexArray(0);
			GL.Disable(EnableCap.Blend);
		}
*/