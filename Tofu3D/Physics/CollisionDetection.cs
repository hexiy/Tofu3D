namespace Tofu3D.Physics;

public static class CollisionDetection
{
    public static bool CheckCollisionRaycastBox(Ray ray, BoxShape boxShape)
    {
        var min = boxShape.GetMinPos();
        var max = boxShape.GetMaxPos();

        var t1 = (min.X - ray.Origin.X) / ray.Direction.X;
        var t2 = (max.X - ray.Origin.X) / ray.Direction.X;
        var t3 = (min.Y - ray.Origin.Y) / ray.Direction.Y;
        var t4 = (max.Y - ray.Origin.Y) / ray.Direction.Y;
        var t5 = (min.Z - ray.Origin.Z) / ray.Direction.Z;
        var t6 = (max.Z - ray.Origin.Z) / ray.Direction.Z;

        var tmin = Mathf.Max(Mathf.Max(Mathf.Min(t1, t2), Mathf.Min(t3, t4)), Mathf.Min(t5, t6));
        var tmax = Mathf.Min(Mathf.Min(Mathf.Max(t1, t2), Mathf.Max(t3, t4)), Mathf.Max(t5, t6));

        // if tmax < 0, ray (line) is intersecting AABB, but whole AABB is behing us
        if (tmax < 0)
        {
            return false;
        }

        // if tmin > tmax, ray doesn't intersect AABB
        if (tmin > tmax)
        {
            return false;
        }

        return true;
    }
}