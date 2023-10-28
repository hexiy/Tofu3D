namespace Tofu3D;

[ExecuteInEditMode]
public class BlockPlacingController : Component, IComponentUpdateable
{
    [Show]
    public GameObject MovingCube;

    public override void Update()
    {
        if (MovingCube != null)
        {
            Vector3 blockPosition = Camera.MainCamera.Transform.WorldPosition +
                                    Camera.MainCamera.Transform
                                        .TransformVectorToWorldSpaceVector(new Vector3(1, 1, 10));
            blockPosition = blockPosition.TranslateToGrid(2);
            MovingCube.Transform.WorldPosition = blockPosition;
        }

        base.Update();
    }
}