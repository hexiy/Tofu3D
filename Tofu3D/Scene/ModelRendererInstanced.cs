using System.IO;
using System.Linq;
using Tofu3D.Rendering.Instancing;
using Vortice.Mathematics;

public class ModelRendererInstanced : Renderer
{
    public override void Awake()
    {
        ObjectInstancingData = new ObjectInstancingData();

        base.Awake();
    }

    public override void OnEnabled()
    {
        ObjectInstancingData.InstancingDataDirty = true;
        ObjectInstancingData.MatrixDirty = true;

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref ObjectInstancingData, remove: true,
            vertexBufferStructureType: VertexBufferStructureType.Model);
        base.OnDisabled();
    }

    public override void SetDefaultMaterial()
    {
        /////////////////////// MESH


        // RuntimeMesh.Mesh.Indices
        if (RuntimeMesh?.Mesh?.PathInLibraryFolder?.Length > 0)
        {
            RuntimeMesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(RuntimeMesh.Mesh.PathInLibraryFolder);
        }
        else
        {
            Asset_Model model =
                Tofu.AssetLoadManager.Load<Asset_Model>(
                    TofuPath.Combine(Folders.BasicModelsInAssets, "defaultCube.obj"));
            RuntimeMesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(model.PathsToMeshAssets.First());

            // RuntimeMesh = null;
        }

        /////////////////////// MATERIAL
        string? pathToObjMaterial = RuntimeMesh?.Mesh?.PathToObjMaterial;

        // for now, always load obj material
        if (pathToObjMaterial != null)
        {
            Material = Tofu.AssetLoadManager.Load<Asset_Material>(pathToObjMaterial);
        }
        else
        {
            if (Material == null || Material?.IsRuntimeCopy == false)
            {
                if (Material?.PathInLibraryFolder.Length == 0 || Material == null)
                {
                    Material = Tofu.AssetLoadManager.Load<Asset_Material>(TofuPath.Combine(Folders.MaterialsInAssets,
                        "ModelRendererInstanced.mat"));
                }
                else
                {
                    Material = Tofu.AssetLoadManager.Load<Asset_Material>(Material.PathInLibraryFolder);
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
                        Material = Tofu.AssetLoadManager.CreateCopyFile(Material);
                    }
                }
            }
        }
    }

    public override void Render()
    {
        // if (this.GameObject.ActiveInHierarchy == false)
        // {
        // return;
        // }

        if (GameObject.IsStatic && ObjectInstancingData.InstancingDataDirty == false &&
            ObjectInstancingData.MatrixDirty == false)
        {
            return;
        }

        if (RuntimeMesh == null || BoxShape == null)
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
            Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref ObjectInstancingData,
                VertexBufferStructureType.Model, isStatic: this.GameObject.IsStatic);
        if (updatedData)
        {
            ObjectInstancingData.InstancingDataDirty = false;
        }
    }
}