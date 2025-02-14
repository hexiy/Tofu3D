using TofuEngine.Rendering.Instancing;

namespace TofuEngine;

public class Particle
{
    public Color Color = Color.White;

    [XmlIgnore] public ObjectInstancingData ObjectInstancingData = new ObjectInstancingData();

    public float Lifetime = 0;
    public Vector3 Size = new Vector3(1);
    public Color SpawnColor;
    public Vector3 Velocity = new Vector3(0, 0, 0);
    public bool Visible = false;
    public Vector3 WorldPosition = new Vector3(0, 0, 0);
}