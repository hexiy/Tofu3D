namespace Tofu3D.Rendering;

public class LightRenderingManager
{
    private int _pointLightsUbo = -1;
    private List<PointLight> _pointLights = new List<PointLight>();
    public int PointLightsCount => _pointLights.Count;

    public LightRenderingManager()
    {
        Scene.ComponentEnabled += ComponentEnabled;
        Scene.ComponentDisabled += ComponentDisabled;
        Scene.SceneDisposed += OnSceneDisposed;
    }

    private void OnSceneDisposed()
    {
        _pointLights.Clear();
        GL.DeleteBuffer(_pointLightsUbo);
        _pointLightsUbo = -1;
    }

    private void ComponentDisabled(Component component)
    {
        if (component is PointLight pointLight)
        {
            _pointLights.Remove(pointLight);
        }
    }

    private void ComponentEnabled(Component component)
    {
        if (component is PointLight pointLight)
        {
            if (_pointLights.Contains(pointLight) == false)
            {
                _pointLights.Add(pointLight);
            }
        }
    }

    public unsafe void BindPointLightsUBO(int shaderProgram)
    {
        if (_pointLightsUbo == -1)
        {
            _pointLightsUbo = GL.GenBuffer();
        }

        int numLights = _pointLights.Count;
        if (numLights == 0)
        {
            return;
        }


        GL.BindBuffer(BufferTarget.UniformBuffer, _pointLightsUbo);

        GL.BufferData(BufferTarget.UniformBuffer, sizeof(PointLightGPUData) * 16 * numLights,
            (IntPtr)null,
            BufferUsageHint.DynamicDraw);

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0,
            _pointLightsUbo); // Bind to binding point 0 (match shader)


        // Find the uniform block index in the shader and link it to the binding point
        int blockIndex = GL.GetUniformBlockIndex(shaderProgram, "LightBuffer");
        if (blockIndex == -1)
        {
            Tofu3D.Debug.LogError("LightBuffer uniform block not found in shader.");
            return;
        }

        GL.UniformBlockBinding(shaderProgram, blockIndex, 0); // Bind block to binding point 0


        // Update UBO with light data
        PointLightGPUData[] lights = new PointLightGPUData[_pointLights.Count]; // Populate with your light data
        for (int i = 0; i < lights.Length; i++)
        {
            PointLightGPUData data = new PointLightGPUData()
            {
                Color = _pointLights[i].Color.ToVector3(),
                Intensity = _pointLights[i].Intensity,
                Position = _pointLights[i].Transform.WorldPosition,
                Radius = _pointLights[i].Radius
            };
            lights[i] = data;
        }

        GL.BindBuffer(BufferTarget.UniformBuffer, _pointLightsUbo);
        GL.BufferSubData(BufferTarget.UniformBuffer, 0, sizeof(PointLightGPUData) * numLights, lights);
    }
}