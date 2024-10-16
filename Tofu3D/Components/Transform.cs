﻿//using Quaternion = Engine.Quaternion;

namespace Scripts;

[ExecuteInEditMode]
public class Transform : Component
{
    private Vector3? _lastFramePosition;

    private Vector3 _localPosition;

    private Vector3 _localScale = Vector3.One;

    private Vector3 _rotation = Vector3.Zero;

    private Vector3 _worldPosition;

    [XmlIgnore] [Hide] public List<Transform> Children = new();

    [Hide] public List<int> ChildrenIDs = new();

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
    [XmlIgnore] public Transform Parent;

    [Hide] public int ParentId = -1;

    public Vector3 Pivot = new(0, 0, 0);

    public override bool CanBeDisabled => false;

    [Hide]
    public Vector3 WorldPosition
    {
        get => _worldPosition;
        set
        {
            _worldPosition = value;

            var calculatedLocalPos = TranslateWorldToLocal(_worldPosition);
            if (_localPosition != calculatedLocalPos)
            {
                _localPosition = calculatedLocalPos;
            }

            //LocalPosition = calculatedLocalPos;
            UpdateChildrenPositions();
        }
    }

    public Vector3 LocalPosition
    {
        get => _localPosition;
        set
        {
            _localPosition = value;

            if (GameObject?.Transform == null)
            {
                return;
            }

            var calculatedWorldPos = TranslateLocalToWorld(_localPosition);
            // local to world is okay
            //Vector3 calculatedLocalPos = TranslateWorldToLocal(calculatedWorldPos);

            if (_worldPosition != calculatedWorldPos)
            {
                _worldPosition = calculatedWorldPos;
            }

            // WorldPosition = calculatedWorldPos;
            UpdateChildrenPositions();
        }
    }

    // [Hide]
    public Vector3 LocalScale
    {
        get => _localScale;
        set
        {
            _localScale = value;
            UpdateChildrenPositions();
        }
    }

    // [XmlIgnore]
    [Hide]
    public Vector3 WorldScale
    {
        get
        {
            var pr = Parent;
            var scl = Transform.LocalScale;

            while (pr != null)
            {
                scl = scl * pr.LocalScale;
                pr = pr.Parent;
            }

            return scl;
        }
        set
        {
            var pr = Parent;
            var parentsScale = Vector3.One;
            while (pr != null)
            {
                parentsScale = parentsScale * pr.LocalScale;
                pr = pr.Parent;
            }

            //LocalScale = value / parentsScale;


            // p (2)
            //	c1 (3)
            //		c2(4)

            // CHILD WORLD SCALE IS 4 * 3 * 2
            // TO SET WORLD SCALE TO 1, we need to set the c2 localScale to something
            // that is 1 / (2 * 3)
            // targetScale / (2*3)
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

    [Hide] public Vector3 WorldRotation => Rotation + GetParentsRotation();

    [Hide] public Vector3 Forward => Transform.GetDirectionFromRotation(Rotation);

    private Vector3 GetParentsRotation() => Parent?.Rotation ?? Vector3.Zero;

    private void UpdateChildrenPositions()
    {
        for (var i = 0; i < Children.Count; i++)
            //Children[i].LocalPosition = Children[i].TranslateWorldToLocal(Children[i].WorldPosition);
        {
            Children[i].WorldPosition = Children[i].TranslateLocalToWorld(Children[i].LocalPosition);
        }
        // setting WorldPosition sets _localPosition too, which just fucks it up
    }

    private Vector3 TranslateLocalToWorld(Vector3 localPos)
    {
        Vector3 worldPos;
        if (Parent)
            // PARENT (1,1)
            // CHILD  (1,1)
            // PARENT SCALE(2,2)
            // CHILD WORLD = 1,1  +      1,1 * 2,2     =        3,3
            // CHILD WORLD = localPos + (Parent.WorldPosition * Parent.LocalScale)
            // worldPos = localPos * Parent.LocalScale + Parent._worldPosition; old
            // parent scale (2,2) world pos(2,2) changing child local pos fks it up
            // PARENT_POS 3
            // PARENT_SCALE 2
            // CHILD_POS 0
            // CHILD_WORLD = 0   +    3*2          =   6
        {
            worldPos = localPos * Parent.WorldScale + Parent.WorldPosition;
        }
        else
        {
            worldPos = localPos;
        }

        return worldPos;
    }

    public Vector3 TranslateWorldToLocal(Vector3 worldPos)
    {
        Vector3 localPos;
        if (Parent)
            // PARENT_POS 3
            // PARENT_SCALE 4
            // WORLD_POS = 12
            // LOCAL_POS should be 0
            // 0      =       12   -  (3*4)
            // 1 = 13 - (3*4)
            // LOCAL_POS = WORLD_POS - (PARENT_POS * PARENT_SCALE)
        {
            localPos = worldPos - Parent.LocalScale * Parent._worldPosition;
        }
        // child moves further with bigger parent position it shouldnt be like that... right?
        // PARENT_POS 10
        // PARENT_SCALE 2
        // WORLD_POS = 13
        // LOCAL_POS should be 3 ??? or 3/2 shenanigans, try both(2/3 too)
        //localPos = (worldPos - Parent._worldPosition);
        else
        {
            localPos = worldPos;
        }

        return localPos;
    }

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
        if (Parent)
        {
            //WorldPosition = TranslateLocalToWorld(LocalPosition);
            //LocalPosition = TranslateWorldToLocal(WorldPosition);
        }

        // if (_lastFramePosition == null)
        // {
        // 	_lastFramePosition = WorldPosition;
        // }
        //
        // Vector3 positionDelta = WorldPosition - _lastFramePosition.Value;
        //
        // if (Children.Count > 0)
        // {
        // 	for (int i = 0; i < Children.Count; i++)
        // 	{
        // 		Children[i].Transform.WorldPosition += positionDelta;
        // 	}
        // }
        //
        // _lastFramePosition = WorldPosition;
    }

    public void RemoveChild(int id)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i].GameObjectId == id)
            {
                Children.RemoveAt(i);
                break;
            }
        }

        for (var i = 0; i < ChildrenIDs.Count; i++)
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

        if (updateTransform)
        {
            Rotation -= par.Transform.Rotation;
            WorldPosition = par.Transform.WorldPosition + (par.Transform.WorldPosition - Transform.WorldPosition);
            //initialAngleDifferenceFromParent = rotation - par.transform.rotation;
        }

        Parent = par;
        ParentId = Parent?.GameObjectId ?? -1;

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

        var transformationMatrix = -Matrix4x4.CreateTranslation(v1)
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
        var transformationMatrix = Matrix4x4.CreateTranslation(dir)
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