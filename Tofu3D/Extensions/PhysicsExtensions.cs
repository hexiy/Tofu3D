namespace Tofu3D;

public static class PhysicsExtensions
{
	public static bool In(this Vector2 point, Shape shape)
	{
		bool isIn = false;
		float distance = 0;
		switch (shape)
		{
			case CircleShape circleCollider:
				if ((distance = Vector2.Distance(circleCollider.Transform.WorldPosition.ToVector2(), point)) < circleCollider.Radius)
				{
					isIn = true;
				}

				break;
			case BoxShape boxCollider:
				Vector2 boxPosition = boxCollider.Transform.WorldPosition;

				//float boxEndX = boxPosition.X + boxCollider.offset.X + (boxCollider.size.X / 2) * boxCollider.transform.pivot.X;

				Vector2 start = boxPosition + boxCollider.Offset * boxCollider.Transform.LocalScale + boxCollider.Size * boxCollider.Transform.Pivot;
				Vector2 end = boxPosition + boxCollider.Offset * boxCollider.Transform.LocalScale + (boxCollider.Size + boxCollider.Size * boxCollider.Transform.Pivot) * boxCollider.Transform.LocalScale;
				isIn = point.X < end.X && point.X > start.X && point.Y < end.Y && point.Y > start.Y;
				break;
		}

		return isIn;
	}
}