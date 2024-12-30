using System.IO;
using System.Linq;

public class ModelRendererInstanced : Renderer
{
    public override void Awake()
    {
        InstancingData = new RendererInstancingData();

        base.Awake();
    }

    public override void OnEnabled()
    {
        InstancingData.InstancingDataDirty = true;
        InstancingData.MatrixDirty = true;

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref InstancingData, remove: true,
            vertexBufferStructureType: VertexBufferStructureType.Model);

        base.OnDisabled();
    }

    public override void SetDefaultMaterial()
    {
        if (Material == null || Material?.IsRuntimeCopy == false)
        {
            if (Material?.PathToRawAsset.Length == 0 || Material == null)
            {
                Material = Tofu.AssetLoadManager.Load<Asset_Material>(Path.Combine(Folders.MaterialsInAssets,"ModelRendererInstanced.mat"));
            }
            else
            {
                Material = Tofu.AssetLoadManager.Load<Asset_Material>(Material.PathToRawAsset);
            }
        }
        else
        {
            if (Material != null)
            {
                Debug.Log(
                    "Not automatically creating material instances, because when tweening higlight box it was losing the reference... only create runtime copy if it was serialized as runtime copy");

                if (Material.IsRuntimeCopy)
                {
                    Material = Material.CreateRuntimeCopy();
                }
            }
        }

        if (RuntimeMesh?.MeshAssetPath.Length > 0)
        {
            RuntimeMesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(RuntimeMesh.MeshAssetPath);
        }
        else
        {
            Asset_Model model =
                Tofu.AssetLoadManager.Load<Asset_Model>(Path.Combine(Folders.ModelsInAssets, "defaultCube.obj"));
            RuntimeMesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(model.PathsToMeshAssets.First());

            // RuntimeMesh = null;
        }
    }

    public override void Render()
    {
        if (this.GameObject.ActiveInHierarchy == false)
        {
            return;
        }

        if (GameObject.IsStatic && InstancingData.InstancingDataDirty == false &&
            InstancingData.MatrixDirty == false)
        {
            return;
        }

        if (RuntimeMesh == null)
        {
            return;
        }

        /*
         bool isTransformHandle = GameObject == TransformHandle.I.GameObject;
        if (isTransformHandle && (Tofu.RenderPassSystem.CurrentRenderPassType != RenderPassType.Opaques && Tofu.RenderPassSystem.CurrentRenderPassType != RenderPassType.UI))
        {
            return;
        }

        if (Transform.IsInCanvas && Tofu.RenderPassSystem.CurrentRenderPassType != RenderPassType.UI || Transform.IsInCanvas == false && Tofu.RenderPassSystem.CurrentRenderPassType == RenderPassType.UI)
        {
            return;
        }

        if (Model == null)
        {
            return;
        }*/

        var updatedData =
            Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref InstancingData,
                VertexBufferStructureType.Model, isStatic: this.GameObject.IsStatic);
        if (updatedData)
        {
            InstancingData.InstancingDataDirty = false;
        }
    }
}