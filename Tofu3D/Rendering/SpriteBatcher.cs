using System.Collections.Generic;
using System.Numerics;

namespace Tofu3D;

public class SpriteBatcher : Batcher
{
	public SpriteBatcher(int size, Material material, Texture texture) : base(size, material, texture)
	{
		VertexAttribSize = 4;
	}

	public override void CreateBuffers()
	{
		Vbo = GL.GenBuffer();

		GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);

		float[] vertices =
		{
			-0.5f, -0.5f, 0, 0,
			0.5f, -0.5f, 1, 0,
			-0.5f, 0.5f, 0, 1,
			-0.5f, 0.5f, 0, 1,
			0.5f, -0.5f, 1, 0,
			0.5f, 0.5f, 1, 1,
		};

		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

		// now we define layout in vao
		Vao = GL.GenVertexArray();

		ShaderCache.BindVertexArray(Vao);

		GL.EnableVertexAttribArray(0);
		GL.EnableVertexAttribArray(1);
		GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false,
		                       sizeof(float) * 4,
		                       (IntPtr) 0);

		GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
		                       sizeof(float) * 4,
		                       (IntPtr) 0);


		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


		// batching
		float[] attribsArray = Attribs.ToArray();

		VboAttribs = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, VboAttribs);
		// bind buffer here(right after generating it), otherwise we get errors..... it was under gl.vertexAttribPointer stuff

		GL.BufferData(target: BufferTarget.ArrayBuffer, size: attribsArray.Length * sizeof(float), data: attribsArray, usage: BufferUsageHint.DynamicDraw); // dynamic draw/streamcopy


		ShaderCache.BindVertexArray(Vao);
		// same vao but new vbo


		GL.EnableVertexAttribArray(2); // vec2(posX,posY)
		GL.EnableVertexAttribArray(3); // vec2(sizeX,sizeY)
		// GL.EnableVertexAttribArray(4); // float(rot)
		//GL.EnableVertexAttribArray(5); // float(col?) 
		GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false,
		                       sizeof(float) * VertexAttribSize,
		                       (IntPtr) 0);// offset is 0, posx and posy are first in the buffer

		GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false,
		                       sizeof(float) * VertexAttribSize,
		                       (IntPtr)0);// offset is 2 floats
		// doesnt rly work?
		// GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false,
		//                        sizeof(float) * VertexAttribSize,
		//                        (IntPtr) 0);
		// GL.VertexAttribPointer(5, 1, VertexAttribPointerType.Float, false,
		//                        sizeof(float) * 1,
		//                        (IntPtr) 0);

		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
	}

	public override void Render()
	{
		if (Material == null)
		{
			return;
		}

		if (Texture == null)
		{
			return;
		}

		CreateBuffers();

		CurrentBufferUploadedSize = Attribs.Count;


		ShaderCache.UseShader(Material.Shader);
		// Material.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity);
		Material.Shader.SetMatrix4X4("u_mvp", Matrix4x4.Identity * Matrix4x4.CreateScale(Units.OneWorldUnit) * Camera.I.ViewMatrix * Camera.I.ProjectionMatrix);

		Material.Shader.SetColor("u_color", Color.White.ToVector4());
		Material.Shader.SetVector2("u_repeats", Vector2.One);

		ShaderCache.BindVertexArray(Vao);

		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


		TextureCache.BindTexture(Texture.Id);

		GL.DrawArrays(PrimitiveType.Triangles, 0, Size);

		Debug.CountStat("Draw Calls", 1);
	}
}