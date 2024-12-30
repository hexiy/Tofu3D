using System.Reflection;

namespace Tofu3D.Scripting;

public class ScriptsReloader
{
    private bool _reloadQueued = false;

    public void QueueScriptsReload()
    {
        _reloadQueued = true;
    }


    public void ReloadScriptsIfNeeded()
    {
        if (_reloadQueued)
        {
            _reloadQueued = false;
        }
        else
        {
            return;
        }

        ScriptsManager.CompileScriptsAssembly();
        Tofu.SceneSerializer.UpdateSerializableTypes();
        Tofu.SceneManager.SaveScene();
        // Tofu.SceneManager.LoadScene();

        Debug.Log("Scripts reloaded");
    }
}