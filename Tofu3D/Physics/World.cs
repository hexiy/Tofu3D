namespace Tofu3D.Physics;

public class World
{
	public List<Rigidbody> Bodies;

	public World()
	{
		I = this;

		Bodies = new List<Rigidbody>();
	}

	public static World I { get; private set; }

	public void AddBody(Rigidbody rb)
	{
		Bodies.Add(rb);
	}
}