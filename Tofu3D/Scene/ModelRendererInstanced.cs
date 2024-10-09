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
        if (Material?.PathToRawAsset.Length == 0 || Material == null)
        {
            Material = Tofu.AssetLoadManager.Load<Asset_Material>("Assets/Materials/ModelRendererInstanced.mat");
        }
        else
        {
            Material = Tofu.AssetLoadManager.Load<Asset_Material>(Material.PathToRawAsset);
        }

        // if (RuntimeMesh?.PathToRawAsset.Length > 0)
        // {
        //     RuntimeMesh = Tofu.AssetManager.Load<Asset_Mesh>(RuntimeMesh.PathToRawAsset);
        // }
        // else
        // {
        //     RuntimeMesh = null;
        // }
    }

    public override void Render()
    {
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
            Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref InstancingData, VertexBufferStructureType.Model);
        if (updatedData)
        {
            InstancingData.InstancingDataDirty = false;
        }
    }
}