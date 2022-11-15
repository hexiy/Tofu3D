namespace Tofu3D;

public class Particle
{
	public Color Color = Color.White;
	public float Lifetime = 0;
	public float Radius = 10;
	public Vector3 Velocity = new(0, 0, 0);
	public bool Visible = false;
	public Vector3 WorldPosition = new(0, 0, 0);
	public Color SpawnColor;
}