using System.IO;

namespace TofuEngine;

public class PlaceholderGameObjectsSpawner
{
    public GameObject SpawnCube()
    {
        GameObject go = GameObject.Create(name: "Cube", runtimeOnly: false, visibleInHierarchy: true);
        go.AddComponent<BoxShape>();
        ModelRenderer modelRenderer = go.AddComponent<ModelRenderer>();
        modelRenderer.NeedsToSetupMesh = false;

        go.Awake();

        PremadeComponentSetupsHelper.PrepareCube(modelRenderer);
        return go;
    }
}