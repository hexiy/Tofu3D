﻿using System.IO;

namespace Tofu3D;

public static class ShaderManager
{
	public static int ShaderInUse = -1;
	public static int VaoInUse = -100;
	static List<string> _shadersReloadQueue = new List<string>();

	public static void Initialize()
	{
		AssetsWatcher.RegisterFileChangedCallback(OnFileChanged, ".glsl");
	}

	static void OnFileChanged(FileChangedInfo fileChangedInfo)
	{
		if (fileChangedInfo.ChangeType is WatcherChangeTypes.Changed)
		{
			QueueShaderReload(fileChangedInfo.Path);
		}
	}

	public static void BindVertexArray(int vao)
	{
		if (vao == VaoInUse)
		{
			return;
		}

		VaoInUse = vao;
		GL.BindVertexArray(vao);
	}

	public static void UseShader(Shader shader)
	{
		UseShader(shader.ProgramId);
	}

	public static void UseShader(int programId)
	{
		if (programId == ShaderInUse)
		{
			return;
		}

		ShaderInUse = programId;
		GL.UseProgram(programId);
	}

	public static void QueueShaderReload(string shaderPath)
	{
		if (_shadersReloadQueue.Contains(shaderPath))
		{
			return;
		}

		_shadersReloadQueue.Add(shaderPath);
	}

	private static void ReloadShader(string shaderPath)
	{
		List<Material> allLoadedMaterials = AssetManager.GetAllLoadedAssetsOfType<Material>();
		foreach (Material loadedMaterial in allLoadedMaterials)
		{
			if (loadedMaterial.Shader?.Path == shaderPath)
			{
				Shader shader = new Shader(shaderPath);

				shader.Load();

				loadedMaterial.SetShader(shader);
			}
		}
		/*// find all Renderer components, and check if the material has the changed shader and reload it, ehh this doesnt work with renderpass shaders for example
		List<Renderer> renderersInScene = SceneManager.CurrentScene.FindComponentsInScene<Renderer>();
		foreach (Renderer renderer in renderersInScene)
		{
			if (renderer.Material?.Shader?.Path == shaderPath)
			{
				Shader shader = new Shader(shaderPath);

				// we might need to call GL from main thread...
				shader.Load();
				renderer.Material?.SetShader(shader);
			}
		}*/
	}

	public static void ReloadQueuedShaders()
	{
		for (int i = 0; i < _shadersReloadQueue.Count; i++)
		{
			ReloadShader(_shadersReloadQueue[i]);
		}

		_shadersReloadQueue.Clear();
	}
}