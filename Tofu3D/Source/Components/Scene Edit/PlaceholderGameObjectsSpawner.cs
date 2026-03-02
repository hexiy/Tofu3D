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

    public GameObject SpawnSprite()
    {
        GameObject go = GameObject.Create(name: "Sprite", runtimeOnly: false, visibleInHierarchy: true);
        go.AddComponent<BoxShape>();
        ModelRenderer modelRenderer = go.AddComponent<ModelRenderer>();
        modelRenderer.NeedsToSetupMesh = false;

        go.Awake();
        go.Transform.Rotation = new Vector3(90, 0, 0);
        PremadeComponentSetupsHelper.PreparePlane(modelRenderer);
        return go;
    }
}