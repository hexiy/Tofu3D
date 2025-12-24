using System.IO;

namespace TofuEngine;

public class ShaderManager
{
    private readonly List<string> _shadersReloadQueue = new List<string>();

    public int VaoInUse = -1;
    private Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();

    public Shader LoadShader(string shaderFile, bool forceReload = false)
    {
        shaderFile = Path.DirectorySeparatorChar + Path.GetRelativePath("/", shaderFile);
        if (_shaders.TryGetValue(shaderFile, out Shader shader))
        {
            if (shader.IsLoaded == false || forceReload == true)
            {
                shader.Load();
            }

            return shader;
        }
        else
        {
            shader = new Shader(shaderFile);

            shader.Load();
            _shaders[shaderFile] = shader;

            return shader;
        }
    }

    public void Initialize()
    {
        Tofu.AssetsWatcher.RegisterFileChangedCallback(OnFileChanged, ".glsl");
    }

    private void OnFileChanged(FileChangedInfo fileChangedInfo)
    {
        if (fileChangedInfo.ChangeType is WatcherChangeTypes.Changed)
        {
            QueueShaderReload(fileChangedInfo.Path);
        }
    }

    public void BindVertexArray(int vao)
    {
        if (vao == VaoInUse)
        {
            return;
        }

        VaoInUse = vao;
        GL.BindVertexArray(vao);
        // Debug.Log("Bind VAO:" + vao);
    }

    public void UseShader(Shader shader, bool forceUse = false)
    {
        if (shader == null)
        {
            Debug.Log("shader is null");
            return;
        }

        if (shader.IsLoaded == false && forceUse == false)
        {
            Debug.LogError("trying to use not loaded shader!!!!!");
            return;
        }

        UseShader(shader.ProgramId);
    }

    public void UseShader(int programId)
    {
        // if (programId == ShaderInUse)
        // {
        //     return;
        // }

        // ShaderInUse = programId;
        GL.UseProgram(programId);
    }

    public void QueueShaderReload(string shaderPath)
    {
        if (_shadersReloadQueue.Contains(shaderPath))
        {
            return;
        }

        _shadersReloadQueue.Add(shaderPath);
    }

    private void ReloadShader(string shaderPath)
    {
        List<Asset_Material> allLoadedMaterials = Tofu.AssetLoadManager.GetAllLoadedAssetsOfType<Asset_Material>();
        foreach (Asset_Material loadedMaterial in allLoadedMaterials)
        {

            string a = Path.GetRelativePath("/", loadedMaterial.Shader?.Path);
            string b = Path.DirectorySeparatorChar+Path.GetRelativePath("/", shaderPath);
            if (a==b) // relativepath to remove ../../
            {
                Shader shader = LoadShader(b, forceReload: true);

                // shader.Load();

                loadedMaterial.Shader = shader;
            }
        }

        if (false)
        {
            // find all Renderer components, and check if the material has the changed shader and reload it, ehh this doesnt work with renderpass shaders for example
            List<Renderer> renderersInScene = Tofu.SceneManager.CurrentScene.FindComponentsInScene<Renderer>();
            foreach (Renderer renderer in renderersInScene)
            {
                if (renderer.Material?.Shader?.Path == shaderPath)
                {
                    Shader shader = new Shader(shaderPath);

                    // we might need to call GL from main thread...
                    shader.Load();
                    renderer.Material.Shader = shader;
                    renderer.Material.LoadShader();
                }
            }
        }
    }

    public void ReloadQueuedShaders()
    {
        for (int i = 0; i < _shadersReloadQueue.Count; i++)
        {
            ReloadShader(_shadersReloadQueue[i]);
        }

        _shadersReloadQueue.Clear();
    }
}