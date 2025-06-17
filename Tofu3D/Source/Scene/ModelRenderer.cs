using System.Linq;
using TofuEngine.Rendering.Instancing;

public class ModelRenderer : Renderer
{
    private bool _isInRenderQueue = true;

    [Show]
    private int StartingIndexInBuffer => ObjectInstancingData?.StartingIndexInBuffer ?? -1;

    public override void Awake()
    {
        ObjectInstancingData = new ObjectInstancingData();

        base.Awake();
    }

    public override void OnEnabled()
    {
        // ObjectInstancingData.InstancingDataDirty = true;
        // ObjectInstancingData.MatrixDirty = true;

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref ObjectInstancingData, remove: true,
            isStatic: this.GameObject.IsStatic);

        base.OnDisabled();
    }

    public override void SetupMeshAndMaterial()
    {
        /////////////////////// MESH
        if (NeedsToSetupMesh)
        {
            // RuntimeMesh.Mesh.Indices
            if (RuntimeMesh?.Mesh?.PathInLibraryFolder?.Length > 0)
            {
                RuntimeMesh = Tofu.AssetLoadManager.Get<RuntimeMesh>(RuntimeMesh.Mesh.PathInLibraryFolder);
            }
            else
            {
                Asset_Model model =
                    Tofu.AssetLoadManager.Get<Asset_Model>(
                        TofuPath.Combine(Folders.BasicModelsInAssets, "defaultCube.obj"));
                RuntimeMesh = Tofu.AssetLoadManager.Get<RuntimeMesh>(model.PathsToMeshAssets.First());

                // RuntimeMesh = null;
            }
        }

        if (NeedsToSetupMaterial)
        {
            /////////////////////// MATERIAL
            string? pathToObjMaterial = RuntimeMesh?.Mesh?.PathToObjMaterial;

            // for now, always load obj material
            if (pathToObjMaterial != null)
            {
                Material = Tofu.AssetLoadManager.Get<Asset_Material>(pathToObjMaterial);
            }
            else
            {
                if (Material == null || Material?.IsRuntimeCopy == false)
                {
                    if (Material?.PathInLibraryFolder.Length == 0 || Material == null)
                    {
                        Material = Tofu.AssetLoadManager.GetDefaultModelRendererInstancedMaterial();
                    }
                    else
                    {
                        Material = Tofu.AssetLoadManager.Get<Asset_Material>(Material.PathInLibraryFolder);
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
    }

    public override void UploadRenderData()
    {
        // UpdateMvp(); // update here because when we have multiple viewports we need to update mvp for each one...

        if (LatestModelMatrix == null)
        {
            return;
        }

        if (GameObject.IsStatic
            && ObjectInstancingData.InstancingDataDirty == false
            && ObjectInstancingData.MatrixDirty == false)
        {
            RemoveFromRenderQueue();

            return;
        }

        if (RuntimeMesh == null || (BoxShape == null && RectTransform == null))
        {
            return;
        }

        if (RuntimeMesh.Mesh?.VerticesCount == 0)
        {
            RemoveFromRenderQueue();


            GameObject.Name = "0 VERTICES?";
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
        bool updatedData =
            Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref ObjectInstancingData,
                // VertexBufferStructureType.Model, 
                isStatic: this.GameObject.IsStatic);
        if (updatedData)
        {
            ObjectInstancingData.InstancingDataDirty = false;
        }

        if (this.GameObject.IsStatic)
        {
            RemoveFromRenderQueue();
        }
    }

    public override void Update()
    {
        if (GameObject.IsStatic == false && _isInRenderQueue == false)
        {
            Tofu.SceneManager.CurrentScene._renderableComponentQueue.AddComponent(this);
            _isInRenderQueue = true;
            // ObjectInstancingData.InstancingDataDirty = true;
            // UploadRenderData();
        }

        base.Update();
    }

    private void RemoveFromRenderQueue()
    {
        Tofu.SceneManager.CurrentScene._renderableComponentQueue.QueueRemove(this);
        _isInRenderQueue = false;
    }
}