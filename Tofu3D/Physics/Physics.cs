namespace Tofu3D.Physics;

public static class Physics
{
    public static RaycastResult Raycast(Ray ray)
    {
        List<Rigidbody> hitBodies = new();


        // Go through all the bodies and their respective colliders and check for collisions
        for (int bodyIndex = 0; bodyIndex < World.I.Bodies.Count; bodyIndex++)
            if (World.I.Bodies[bodyIndex].Shape is BoxShape)
            {
                bool hit = CollisionDetection.CheckCollisionRaycastBox(ray,
                    World.I.Bodies[bodyIndex].Shape as BoxShape);
                if (hit) hitBodies.Add(World.I.Bodies[bodyIndex]);
            }

        RaycastResult result = new() { HitBodies = hitBodies };

        return result;
    }
}