public class Asset_Model : Asset<Asset_Model>
{
    public List<string> PathsToMeshAssets = new List<string>();

    public RuntimeMesh GetMesh(int index)
    {
        if (index >= PathsToMeshAssets.Count)
        {
            Debug.LogError($"Mesh index {index} doesnt exist");
            return null;
        }

        return Tofu.AssetLoadManager.Get<RuntimeMesh>(PathsToMeshAssets[index]);
    }
}