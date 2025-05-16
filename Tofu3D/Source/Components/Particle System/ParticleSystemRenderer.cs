using System.Linq;

public class ParticleSystemRenderer : Renderer
{
    private ParticleSystem _particleSystem;

    public void SetParticleSystem(ParticleSystem particleSystem)
    {
        _particleSystem = particleSystem;
        SetParticlesInstancingDataDirty();
    }

    private void SetParticlesInstancingDataDirty()
    {
        if (_particleSystem == null)
        {
            return;
        }

        foreach (var particle in _particleSystem?.Particles)
        {
            particle.ObjectInstancingData.InstancingDataDirty = true;
            particle.ObjectInstancingData.MatrixDirty = true;
        }
    }

    private void RemoveAllParticlesFromInstancedRenderingSystem()
    {
        foreach (var particle in _particleSystem?.Particles)
        {
            Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref particle.ObjectInstancingData, remove: true);
        }
    }

    public override void OnEnabled()
    {
        SetParticlesInstancingDataDirty();

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        RemoveAllParticlesFromInstancedRenderingSystem();

        base.OnDisabled();
    }

    public override void UploadRenderData()
    {
        UpdateModelMatrix();
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

        if (RuntimeMesh == null)
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
        foreach (Particle particle in _particleSystem.Particles)
        {
            var particleModelMatrix = Matrix4x4.CreateScale(particle.Size * (particle.Visible ? 1 : 0)) *
                                      Matrix4x4.CreateTranslation(particle.WorldPosition * Transform.WorldScale);

            Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref particle.ObjectInstancingData, particleModelMatrix,
                color: particle.Color);
        }
    }

    public override void SetDefaultMaterial()
    {
        /////////////////////// MESH


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
                    Material = Tofu.AssetLoadManager.Get<Asset_Material>(TofuPath.Combine(Folders.MaterialsInAssets,
                        "ModelRendererInstanced.mat"));
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

    private void RemoveFromRenderQueue()
    {
        Tofu.SceneManager.CurrentScene._renderableComponentQueue.QueueRemove(this);
    }
}