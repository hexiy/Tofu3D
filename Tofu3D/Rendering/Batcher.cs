using System.Collections.Generic;

namespace Tofu3D;

public abstract class Batcher
{
	public enum BatcherType
	{
		Sprite,
		SpriteSheet
	}

	internal List<float> Attribs = new(1000);
	internal int CurrentBufferUploadedSize = 0;
	public Material Material;
	internal Dictionary<int, int> RendererLocationsInAttribs = new(); // key:renderer ID, value:index in attribs list

	internal int Size;
	public Texture Texture;
	internal int Vao = -1;
	internal int Vbo = -1;
	internal int VboAttribs = -1;
	internal int VertexAttribSize;

	public Batcher(int size, Material material, Texture texture)
	{
		this.Size = size;
		this.Material = material;
		this.Texture = texture;
		Attribs = new List<float>();
		for (int i = 0; i < 1000; i++)
		{
			Attribs.Add(0);
		}
	}

	public abstract void CreateBuffers();

	public abstract void Render();

	int _instanceCount = 0;
	public void AddGameObject(int gameObjectId, int instanceIndex = 0)
	{
		int index = gameObjectId;
		if (instanceIndex != 0)
		{
			index = -gameObjectId - instanceIndex * VertexAttribSize;
		}

		if (RendererLocationsInAttribs.ContainsKey(index))
		{
			return;
		}

		RendererLocationsInAttribs.Add(index,_instanceCount);

		float[] att = new float[VertexAttribSize];

		SetAttribs(gameObjectId, att);
		_instanceCount++;
	}

	public void SetAttribs(int gameObjectId, float[] attribs, int instanceIndex = 0)
	{
		int index = gameObjectId;
		if (instanceIndex != 0)
		{
			index = -gameObjectId - instanceIndex * VertexAttribSize;
		}

		for (int i = 0; i < 6; i++) // for evvery vertex
		{
			for (int j = 0; j < VertexAttribSize; j++)
			{
				Attribs[RendererLocationsInAttribs[index] + i * VertexAttribSize + j] = attribs[j];
			}
		}
	}
}