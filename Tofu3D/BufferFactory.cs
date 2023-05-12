using System.Linq;

namespace Tofu3D;

public static class BufferFactory
{
	public static void CreateBufferForShader(Material material)
	{
		if (material.Shader.BufferType == BufferType.Rendertexture)
		{
			CreateRenderTextureBuffers(ref material.Vao);
		}

		if (material.Shader.BufferType == BufferType.Sprite)
		{
			CreateRenderTextureBuffers(ref material.Vao);
		}

		if (material.Shader.BufferType == BufferType.Model)
		{
			// CreateModelBuffers(ref material.Vao);
		}

		if (material.Shader.BufferType == BufferType.Cubemap)
		{
			CreateCubemapBuffers(ref material.Vao);
		}
	}

	public static void CreateRenderTextureBuffers(ref int vao)
	{
		float[] vertices =
		{
			-1.0f, 1.0f, 0.0f, 1.0f,
			-1.0f, -1.0f, 0.0f, 0.0f,
			1.0f, -1.0f, 1.0f, 0.0f,
			-1.0f, 1.0f, 0.0f, 1.0f,
			1.0f, -1.0f, 1.0f, 0.0f,
			1.0f, 1.0f, 1.0f, 1.0f
		};


		vao = GL.GenVertexArray();
		ShaderManager.BindVertexArray(vao);


		VertexBuffer vertexBuffer = VertexBuffer.Create<float>(BufferTarget.ArrayBuffer, vertexData: vertices, elementsPerVertex: 4);
		vertexBuffer.EnableAttribs(sequential: false, 2, 2);
	}

	public static void CreateModelBuffers(ref int vao, float[] vertexBufferData, int[] countsOfElements)
	{
		GL.Enable(EnableCap.DepthTest);

		vao = GL.GenVertexArray();
		GL.BindVertexArray(vao);

		VertexBuffer vertexBuffer = VertexBuffer.Create<float>(BufferTarget.ArrayBuffer, vertexData: vertexBufferData, elementsPerVertex: 8);
		vertexBuffer.EnableAttribs(sequential: true, countsOfElements);


		/*VertexBuffer instanceBuffer = VertexBuffer.Create<float>(BufferTarget.ArrayBuffer, vertexData: translations, elementsPerVertex: 8);
		
		GL.EnableVertexAttribArray(2);
		vertexBuffer.EnableAttribs(sequential: true, countsOfElements);*/

		GL.BindVertexArray(0);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
	}

	public static void CreateCubemapBuffers(ref int vao)
	{
		GL.Enable(EnableCap.DepthTest);
		float[] vertices = new float[]
		                   {
			                   -1.0f, -1.0f, 1.0f, // 0        7-----------6
			                   1.0f, -1.0f, 1.0f, // 1       /|          /|
			                   1.0f, -1.0f, -1.0f, // 2      4-----------5 |
			                   -1.0f, -1.0f, -1.0f, // 3      | |         | |
			                   -1.0f, 1.0f, 1.0f, // 4      | 3---------|-2
			                   1.0f, 1.0f, 1.0f, // 5      |/          |/
			                   1.0f, 1.0f, -1.0f, // 6      0-----------1
			                   -1.0f, 1.0f, -1.0f // 7
		                   };

		uint[] indices = new uint[]
		                 {
			                 // Right
			                 1, 2, 6,
			                 6, 5, 1,
			                 // Left
			                 0, 4, 7,
			                 7, 3, 0,
			                 // Top
			                 4, 5, 6,
			                 6, 7, 4,
			                 // Bottom
			                 0, 3, 2,
			                 2, 1, 0,
			                 // Back
			                 0, 1, 5,
			                 5, 4, 0,
			                 // Front
			                 3, 7, 6,
			                 6, 2, 3
		                 };
		vao = GL.GenVertexArray();
		GL.BindVertexArray(vao);

		VertexBuffer vertexBuffer = VertexBuffer.Create<float>(BufferTarget.ArrayBuffer, vertexData: vertices, elementsPerVertex: 3);
		vertexBuffer.EnableAttribs(sequential: false, 3); // xyz

		VertexBuffer indexBuffer = VertexBuffer.Create<uint>(BufferTarget.ElementArrayBuffer, vertexData: indices, elementsPerVertex: 3);
	}
	/*private static void CreateSpriteRendererBuffers(ref int vao, ref int vbo)
	{
		vao = GL.GenVertexArray();
		vbo = GL.GenBuffer();

		BindVAO(vao);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

		List<float> verticesList = new List<float>();

		// 100 limit for now, but it can be dynamic too
		for (int i = 0; i < 1000; i++)
			verticesList.AddRange(new[]
			                      {
				                      -0.5f, -0.5f, 0, 0,
				                      0.5f, -0.5f, 1, 0,
				                      -0.5f, 0.5f, 0, 1,

				                      -0.5f, 0.5f, 0, 1,
				                      0.5f, -0.5f, 1, 0,
				                      0.5f, 0.5f, 1, 1
			                      });

		float[] vertices = verticesList.ToArray();

		GL.NamedBufferStorage(
		                      vbo,
		                      sizeof(float) * vertices.Length,
		                      vertices,
		                      BufferStorageFlags.MapWriteBit);

		// ATTRIB: vertex position -   2 floats
		GL.VertexArrayAttribBinding(vao, 0, 0);
		GL.EnableVertexArrayAttrib(vao, 0);
		GL.VertexArrayAttribFormat(
		                           vao,
		                           0, // attribute index, from the shader location = 0
		                           2, // size of attribute, vec2
		                           VertexAttribType.Float, // contains floats
		                           false,
		                           0); // relative offset, first item

		// ATTRIB: texture coord -  2 floats
		GL.VertexArrayAttribBinding(vao, 1, 0);
		GL.EnableVertexArrayAttrib(vao, 1);
		GL.VertexArrayAttribFormat(
		                           vao,
		                           1, // attribute index, from the shader location = 0
		                           2, // size of attribute, vec2
		                           VertexAttribType.Float, // contains floats
		                           true,
		                           8); // relative offset, first item

		GL.VertexArrayVertexBuffer(vao, 0, vbo, new IntPtr(0), sizeof(float) * 4);

		bool batching = true;
		if (batching)
		{
			//
			//
			//
			//
			// create new vertex buffer for positions
			int vbo_positions = GL.GenBuffer();

			BindVAO(vao);
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_positions);

			List<float> attribsList = new List<float>();

			// 100 limit for now, but it can be dynamic too\
			int x = 0;
			int y = 0;
			for (int i = 0; i < 1000; i++)
			{
				attribsList.AddRange(new float[]
				                     {
					                     x, y, 100, 100, 0xFFFFFF,
					                     x, y, 100, 100, 0xFFFFFF,
					                     x, y, 100, 100, 0xFFFFFF,

					                     x, y, 100, 100, 0xFFFFFF,
					                     x, y, 100, 100, 0xFFFFFF,
					                     x, y, 100, 100, 0xFFFFFF
				                     });

				x += 100;
				if (i % 40 == 0)
				{
					x = 0;
					y += 100;
				}
			}

			float[] attribs = attribsList.ToArray();

			GL.NamedBufferStorage(
			                      vbo_positions,
			                      sizeof(float) * attribs.Length,
			                      attribs,
			                      BufferStorageFlags.MapWriteBit);
			// ATTRIB: vertex position -   2 floats
			GL.VertexArrayAttribBinding(vao, 2, 1);
			GL.EnableVertexArrayAttrib(vao, 2);
			GL.VertexArrayAttribFormat(
			                           vao,
			                           2, // attribute index, from the shader location = 0
			                           2, // size of attribute, vec2
			                           VertexAttribType.Float, // contains floats
			                           false,
			                           0); // relative offset, first item


			// ATTRIB: size -   2 floats
			GL.VertexArrayAttribBinding(vao, 3, 1);
			GL.EnableVertexArrayAttrib(vao, 3);
			GL.VertexArrayAttribFormat(
			                           vao,
			                           3, // attribute index, from the shader location = 0
			                           2, // size of attribute, vec2
			                           VertexAttribType.Float, // contains floats
			                           false,
			                           8); // relative offset, first item

			// ATTRIB: color -   1 int
			GL.VertexArrayAttribBinding(vao, 4, 1);
			GL.EnableVertexArrayAttrib(vao, 4);
			GL.VertexArrayAttribFormat(
			                           vao,
			                           4, // attribute index, from the shader location = 0
			                           1, // size of attribute, vec2
			                           VertexAttribType.UnsignedInt, // contains floats
			                           true,
			                           16); // relative offset, first item


			GL.VertexArrayVertexBuffer(vao, 1, vbo_positions, IntPtr.Zero, sizeof(float) * 4 + sizeof(byte));
		}
	}*/
}