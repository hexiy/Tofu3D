//using Quaternion = Engine.Quaternion;

namespace Scripts;

[ExecuteInEditMode]
public class Transform : Component
{
    private Vector3? _lastFramePosition;

    private Vector3 _localPosition;

    private Vector3 _localScale = Vector3.One;

    private Vector3 _rotation = Vector3.Zero;

    private Vector3 _worldPosition;

    [XmlIgnore]
    [Hide]
    public List<Transform> Children = new();

    [Hide]
    public List<int> ChildrenIDs = new();

    public bool MockIsInCanvas = false;
    //[Hide] public Vector3 localPosition { get { return position - GetParentPosition(); } set { position = GetParentPosition() + value; } }
    //[Hide] public Vector3 initialAngleDifferenceFromParent = Vector3.Zero;
    //[Hide] public Vector3 up { get { return position + TransformVector(new Vector3(0, 1, 0)); } }

    /*[ShowInEditor]
    public Vector3 LocalPosition
    {
        get { return transform.position - GetParentPosition(); }
        set
        {
            position = value + GetParentPosition();
            localPosition = value;
        }
    }*/
    [XmlIgnore]
    public Transform Parent;

    [Hide]
    public int ParentId = -1;
    
    public override bool CanBeDisabled => false;

    [Hide]
    public Vector3 WorldPosition
    {
        get
        {
            Transform? parent = Parent;
            _worldPosition = LocalPosition; // Start with local position
            if (parent != null)
            {
                Matrix4x4 allParentsMatrix = Matrix4x4.Identity;
                while (parent != null)
                {
                    allParentsMatrix = Matrix4x4.Multiply(allParentsMatrix, parent.Matrix);
                    parent = parent.Parent;
                }

                // Create a local transformation matrix for this transform
                Matrix4x4 localMatrix = this.MatrixLocalPosition;

                // Combine the parent's matrix with the local matrix to transform to world space
                Matrix4x4 combinedMatrix = Matrix4x4.Multiply(localMatrix, allParentsMatrix);

                // Extract the world position from the combined matrix
                _worldPosition = new Vector3(combinedMatrix.M41, combinedMatrix.M42, combinedMatrix.M43);
            }


            return _worldPosition;
        }
        set
        {
            Transform? parent = Parent;
            while (parent != null)
            {
                // Calculate the inverse transformation matrix for the parent
                bool success = Matrix4x4.Invert(Parent.Matrix, out Matrix4x4 inverseParentMatrix);
                if (success)
                {
                    // Transform the world position back into the local space of the parent
                    Vector3 localPos = Vector3.Transform(value, inverseParentMatrix);
                    LocalPosition = new Vector3(localPos.X, localPos.Y, localPos.Z);
                }

                parent = parent.Parent;
            }

            if (Parent == null)
            {
                // If there's no parent, world position directly translates to local position
                LocalPosition = value;
            }
        }
    }

    public Vector3 LocalPosition
    {
        get => _localPosition;
        set => _localPosition = value;
    }

    // [Hide]
    public Vector3 LocalScale
    {
        get => _localScale;
        set
        {
            _localScale = value;
            // UpdateChildrenPositions();
        }
    }

    // [XmlIgnore]
    [Hide]
    public Vector3 WorldScale
    {
        get
        {
            Transform? pr = Parent;
            Vector3 scl = Transform.LocalScale;

            while (pr != null)
            {
                scl = scl * pr.LocalScale;
                pr = pr.Parent;
            }

            return scl;
        }
        set
        {
            Transform? pr = Parent;
            Vector3 parentsScale = Vector3.One;
            while (pr != null)
            {
                parentsScale = parentsScale * pr.LocalScale;
                pr = pr.Parent;
            }
        }
    }

    [Show]
    internal bool IsInCanvas => MockIsInCanvas; /*
    {
        get { return Transform.Parent?.GetComponent<Canvas>() != null || MockIsInCanvas; }
    }*/

    public Vector3 Rotation
    {
        get => _rotation;
        set => _rotation = new Vector3(value.X % 360, value.Y % 360, value.Z % 360);
    }

    [Hide]
    public Vector3 WorldRotation => Rotation + GetParentsRotation();

    [Hide]
    public Vector3 ForwardWorldDirection => Transform.GetDirectionFromRotation(Rotation);

    private Vector3 GetParentsRotation()
    {
        Transform parent = Parent;
        Vector3 rotationAccumulative = Vector3.Zero;
        while (parent != null)
        {
            rotationAccumulative += parent.Rotation;
            parent = parent.Parent;
        }

        return rotationAccumulative;
    }

    private Matrix4x4 Matrix => MatrixLocalScale * MatrixLocalRotation * MatrixLocalPosition;

    private Matrix4x4 MatrixLocalScale => Matrix4x4.CreateScale(LocalScale);
    private Matrix4x4 MatrixLocalPosition => Matrix4x4.CreateTranslation(LocalPosition);

    private Matrix4x4 MatrixLocalRotation => Matrix4x4.CreateFromYawPitchRoll(
        Rotation.Y / 180 * Mathf.Pi,
        Rotation.X / 180 * Mathf.Pi,
        Rotation.Z / 180 * Mathf.Pi);

    public override void Awake()
    {
        //LocalPosition = LocalPosition;
        base.Awake();
    }

    public override void EditorUpdate()
    {
        Update();
    }

    public void Update()
    {
    }

    public void RemoveChild(int id)
    {
        for (int i = 0; i < Children.Count; i++)
        {
            if (Children[i].GameObjectId == id)
            {
                Children.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; i < ChildrenIDs.Count; i++)
        {
            if (ChildrenIDs[i] == id)
            {
                ChildrenIDs.RemoveAt(i);
                break;
            }
        }
    }

    public void SetParent(Transform par, bool updateTransform = false)
    {
        // if (ParentId != -1 && Tofu.SceneManager.CurrentScene.GetGameObject(ParentId) != null)
        // {
        // 	Tofu.SceneManager.CurrentScene.GetGameObject(ParentId).Transform.RemoveChild(GameObjectId);
        // }

        if (Transform.Parent != null && Transform.Parent != par)
        {
            Transform.Parent.RemoveChild(GameObjectId);
        }

        Parent = par;
        ParentId = Parent?.GameObjectId ?? -1;

        if (updateTransform)
        {
            Rotation -= par.Transform.Rotation;
            WorldPosition = par.Transform.WorldPosition + (par.Transform.WorldPosition - Transform.WorldPosition);
            //initialAngleDifferenceFromParent = rotation - par.transform.rotation;
        }

        if (par != null)
        {
            par.Children.Add(this);
            par.ChildrenIDs.Add(GameObjectId);
        }
    }

    public Vector3 GetParentPosition()
    {
        if (Parent != null)
        {
            return Parent.Transform.WorldPosition;
        }

        return Vector3.Zero;
    }

    public static Vector3 RotateVectorByRotation(Vector3 v1, Vector3 v2)
    {
        // v1 = new Vector3(-v1.X, -v1.Y, -v1.Z);

        Matrix4x4 transformationMatrix = -Matrix4x4.CreateTranslation(v1)
                                         * Matrix4x4.CreateRotationX(v2.X / 180 * Mathf.Pi)
                                         * Matrix4x4.CreateRotationY(v2.Y / 180 * Mathf.Pi)
                                         * Matrix4x4.CreateRotationZ(v2.Z / 180 * Mathf.Pi);

        Vector3 x = transformationMatrix.Translation;
        return x;
    }

    public Vector3 GetDirectionFromRotation(Vector3 rotation)
    {
        float pitch = rotation.X;
        float yaw = rotation.Y;
        float pitchRadians = MathHelper.DegreesToRadians(pitch);
        float yawRadians = MathHelper.DegreesToRadians(yaw);

        Vector3 direction;
        direction.Z = (float)(Math.Cos(pitchRadians) * Math.Cos(yawRadians));
        direction.Y = (float)Math.Sin(pitchRadians);
        direction.X = (float)(Math.Cos(pitchRadians) * Math.Sin(yawRadians));

        return direction.Normalized();


        /*var radiansX = rotation.X / 180 * Mathf.Pi;
        var radiansY = rotation.Y / 180 * Mathf.Pi;
        var radiansZ = rotation.Z / 180 * Mathf.Pi;

        // Create the transformation matrix based on the rotation
        var transformationMatrix =
            Matrix4x4.CreateRotationX(radiansX) *
            Matrix4x4.CreateRotationY(radiansY) *
            Matrix4x4.CreateRotationZ(radiansZ);

        // Apply the transformation to the forward direction
        var forward = new Vector3(0, 0, 1);
        var direction = Vector3.Transform(forward, transformationMatrix);

        return direction;*/
    }

    public Vector3 TransformVectorToWorldSpaceVector(Vector3 dir)
    {
        // dir = dir.Normalized();
        // Matrix4x4 transformationMatrix = (Matrix4x4.CreateTranslation(new Vector3(0, 0, 1))
        //                                 * Matrix4x4.CreateRotationX(Transform.Rotation.X / 180 * Mathf.Pi)
        //                                 * Matrix4x4.CreateRotationY(Transform.Rotation.Y / 180 * Mathf.Pi)
        //                                 * Matrix4x4.CreateRotationZ(Transform.Rotation.Z / 180 * Mathf.Pi));
        //
        // Vector3 x = transformationMatrix.Translation;
        //
        // return x;
        // dir = new Vector3(-dir.X, -dir.Y, -dir.Z);
        //dir = dir.Normalized();
        Matrix4x4 transformationMatrix = Matrix4x4.CreateTranslation(dir)
                                         * Matrix4x4.CreateRotationX(Transform.WorldRotation.X / 180 * Mathf.Pi)
                                         * Matrix4x4.CreateRotationY(Transform.WorldRotation.Y / 180 * Mathf.Pi)
                                         * Matrix4x4.CreateRotationZ(Transform.WorldRotation.Z / 180 * Mathf.Pi);

        Vector3 x = transformationMatrix.Translation;

        return x;
    }

    // public Vector3 TransformVector(Vector3 dir)
    // {
    // 	Vector3 direction = new Vector3(
    // 	                                (float) (MathHelper.Sin(MathHelper.DegreesToRadians(Transform.Rotation.Y))
    // 	                                       * MathHelper.Cos(MathHelper.DegreesToRadians(Transform.Rotation.X))),
    // 	                                (float) (MathHelper.Sin(MathHelper.DegreesToRadians(Transform.Rotation.X))),
    // 	                                (float) (MathHelper.Cos(MathHelper.DegreesToRadians(Transform.Rotation.Y))
    // 	                                       * MathHelper.Cos(MathHelper.DegreesToRadians(Transform.Rotation.X)))
    // 	                               );
    //
    // 	direction = direction.Normalized();
    //
    // 	Matrix4x4 mat = Matrix4x4.CreateTranslation(direction) * Matrix4x4.CreateLookAt(Vector3.Zero, dir, Vector3.Up);
    //
    // 	direction = mat.Translation;
    // 	direction = direction.Normalized();
    // 	return dir;
    // }
}