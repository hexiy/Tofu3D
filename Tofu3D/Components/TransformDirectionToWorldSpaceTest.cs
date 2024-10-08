public class TransformDirectionToWorldSpaceTest : Component
{
    [ExecuteInEditMode]
    public override void Start()
    {
        DoTest();

        base.Start();
    }

    private void DoTest()
    {
        Transform.WorldPosition = Vector3.Zero;

        Vector3 v1 = new(0, 0, 1);

        // Vector3 result1 = Transform.TransformDirectionToWorldSpace(v1);
        // Vector3 wantedResult1 = new Vector3(0, 0, 1);
        // Debug.Log($"[{result1 == wantedResult1}] Wanted {wantedResult1} got {result1}");
        //
        // Transform.Rotation = new Vector3(0, 90, 0);
        // Vector3 result2 = Transform.TransformDirectionToWorldSpace(v1);
        // Vector3 wantedResult2 = new Vector3(1, 0, 0);
        // Debug.Log($"[{result2 == wantedResult2}] Wanted {wantedResult2} got {result2}");
        //
        // Transform.Rotation = new Vector3(0, 180, 0);
        // Vector3 result3 = Transform.TransformDirectionToWorldSpace(v1);
        // Vector3 wantedResult3 = new Vector3(0, 0, -1);
        // Debug.Log($"[{result3 == wantedResult3}] Wanted {wantedResult3} got {result3}");
        {
            Transform.Rotation = new Vector3(90, 0, 0);
            var result = Transform.TransformVectorToWorldSpaceVector(v1);
            Vector3 wantedResult = new(0, -1, 0);
            Debug.Log($"[{result == wantedResult}] Wanted {wantedResult} got {result}");
        }
    }
}