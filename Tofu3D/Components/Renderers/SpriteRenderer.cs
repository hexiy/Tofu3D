using System.IO;
using Tofu3D.Components.Renderers;

namespace Tofu3D;

public class SpriteRenderer : TextureRenderer
{
	[Hide] public virtual bool Batched { get; set; } = false;

	public override void Awake()
	{
		SetDefaultTexture(Path.Combine(Folders.Textures, "solidColor.png"));

		SetNativeSize += () => { UpdateBoxShapeSize(); };
		CreateMaterial();
		if (Texture == null)
		{
			Texture = new Texture();
		}
		else
		{
			LoadTexture(Texture.Path);
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
			BoxShape.Size = Texture.Size / Units.OneWorldUnit;
		}
	}

	public override void CreateMaterial()
	{
		if (Material == null)
		{
			Material = MaterialCache.GetMaterial("SpriteRenderer");
		}

		base.CreateMaterial();
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
			BatchingManager.UpdateAttribs(Texture.Id, GameObjectId, Transform.WorldPosition,
			                              new Vector2(GetComponent<BoxShape>().Size.X * Transform.WorldScale.X,
			                                          GetComponent<BoxShape>().Size.Y * Transform.WorldScale.Y),
			                              Transform.Rotation.Z, Color);
			return;
		}

		ShaderCache.UseShader(Material.Shader);
		Material.Shader.SetVector2("u_resolution", Texture.Size);
		Material.Shader.SetMatrix4X4("u_mvp", LatestModelViewProjection);
		Material.Shader.SetColor("u_color", Color.ToVector4());
		Material.Shader.SetVector2("u_repeats", Repeats);

		ShaderCache.BindVertexArray(Material.Vao);

		if (Material.Additive)
		{
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
		}
		else
		{
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		}

		TextureCache.BindTexture(Texture.Id);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		Debug.CountStat("Draw Calls", 1);
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