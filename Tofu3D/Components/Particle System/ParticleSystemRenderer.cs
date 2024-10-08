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
        foreach (var particle in _particleSystem?.Particles)
        {
            particle.InstancingData.InstancingDataDirty = true;
            particle.InstancingData.MatrixDirty = true;
        }
    }

    private void RemoveAllParticlesFromInstancedRenderingSystem()
    {
        foreach (var particle in _particleSystem?.Particles)
        {
            Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref particle.InstancingData, remove: true,
                vertexBufferStructureType: VertexBufferStructureType.Model);
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

    public override void SetDefaultMaterial()
    {
        if (Material?.Path.Length == 0 || Material == null)
        {
            Material = Tofu.AssetManager.Load<Asset_Material>("Assets/Materials/ModelRendererInstanced.mat");
        }
        else
        {
            Material = Tofu.AssetManager.Load<Asset_Material>(Material.Path);
        }

        if (AssetMesh?.Path.Length > 0)
        {
            AssetMesh = Tofu.AssetManager.Load<Asset_Mesh>(AssetMesh.Path);
        }
        else
        {
            AssetMesh = null;
        }
    }

    public override void Render()
    {
        if (GameObject.IsStatic && InstancingData.InstancingDataDirty == false &&
            InstancingData.MatrixDirty == false)
        {
            return;
        }

        if (AssetMesh == null)
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


        for (var i = 0; i < _particleSystem.Particles.Count; i++)
        {
            var particle = _particleSystem.Particles[i];


            var particleModelMatrix = Matrix4x4.CreateScale(particle.Size * (particle.Visible ? 1 : 0)) *
                                      Matrix4x4.CreateTranslation(particle.WorldPosition * Transform.WorldScale);

            Tofu.InstancedRenderingSystem.UpdateObjectData(this, ref particle.InstancingData,
                VertexBufferStructureType.Model, particleModelMatrix,
                color: particle.Color);

            if (particle.Visible == false)
            {
                _particleSystem.DisableParticle(i);
            }
            // return;
        }
        // bool updatedData = Tofu.InstancedRenderingSystem.UpdateObjectData(this,);
        // if (updatedData)
        // {
        // InstancingData.InstancingDataDirty = false;
        // }
    }
}