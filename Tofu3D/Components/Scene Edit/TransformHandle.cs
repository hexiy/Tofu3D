namespace Tofu3D;

[ExecuteInEditMode]
public class TransformHandle : Component, IComponentUpdateable
{
    public enum Axis
    {
        X,
        Y,
        Z,
        Xy
    }

    private List<Transform> _selectedTransforms;
    public BoxShape BoxColliderX;
    public BoxShape BoxColliderXy;
    public BoxShape BoxColliderY;
    public BoxShape BoxColliderZ;

    public bool Clicked;
    public Axis? CurrentAxisSelected;
    public ModelRendererInstanced ModelRendererX;
    public ModelRendererInstanced ModelRendererXy;
    public ModelRendererInstanced ModelRendererY;
    public ModelRendererInstanced ModelRendererZ;

    public bool ObjectSelected;
    public static TransformHandle I { get; private set; }

    public void Update()
    {
        if (Camera.MainCamera.IsOrthographic)
        {
            Transform.LocalScale = Vector3.One * Camera.MainCamera.OrthographicSize * 1.5f;
        }
        else
        {
            Transform.LocalScale = Vector3.One *
                                   Vector3.Distance(Transform.WorldPosition,
                                       Camera.MainCamera.Transform.WorldPosition) * 0.2f;
        }

        if (Tofu.MouseInput.ButtonReleased())
        {
            CurrentAxisSelected = null;
            Clicked = false;
        }

        if (Tofu.MouseInput.ButtonPressed())
        {
            Clicked = false;
        }

        // if (MousePickingSystem.HoveredRenderer == ModelRendererX)
        // {
        // 	CurrentAxisSelected = Axis.X;
        // 	Clicked = true;
        // }
        //
        // if (MousePickingSystem.HoveredRenderer == ModelRendererY)
        // {
        // 	CurrentAxisSelected = Axis.Y;
        // 	Clicked = true;
        // }
        //
        // if (MousePickingSystem.HoveredRenderer == ModelRendererZ)
        // {
        // 	CurrentAxisSelected = Axis.Z;
        // 	Clicked = true;
        // }
        //
        // if (MousePickingSystem.HoveredRenderer == ModelRendererXy)
        // {
        // 	CurrentAxisSelected = Axis.Xy;
        // 	Clicked = true;
        // }
        if (Tofu.MouseInput.IsButtonDown() && GameObject.ActiveInHierarchy && Clicked)
        {
            SetSelectedObjectRigidbodyAwake(false);
            // Move(Tofu.MouseInput.WorldDelta);
            Move(Tofu.MouseInput
                .ScreenDelta); // /_selectedTransforms[0].GetComponent<Renderer>().DistanceFromCamera * 1000f);
        }
        else
        {
            SetSelectedObjectRigidbodyAwake(true);
        }

        if (ObjectSelected == false || _selectedTransforms == null)
            //GameObject.Active = false;
        {
            return;
        }

        Transform.WorldPosition = GetCenterOfSelection();
        Transform.Rotation = GetRotationOfSelection();
        // if (MousePickingSystem.HoveredRenderer == ModelRendererX || CurrentAxisSelected == Axis.X)
        // {
        // 	ModelRendererX.Color = Color.WhiteSmoke;
        // }
        // else
        // {
        // 	ModelRendererX.Color = Color.Red;
        // }
        //
        // if (MousePickingSystem.HoveredRenderer == ModelRendererY || CurrentAxisSelected == Axis.Y)
        // {
        // 	ModelRendererY.Color = Color.WhiteSmoke;
        // }
        // else
        // {
        // 	ModelRendererY.Color = Color.YellowGreen;
        // }
        //
        // if (MousePickingSystem.HoveredRenderer == ModelRendererXy || CurrentAxisSelected == Axis.Xy)
        // {
        // 	ModelRendererXy.Color = Color.WhiteSmoke;
        // }
        // else
        // {
        // 	ModelRendererXy.Color = Color.Gold;
        // }
        //
        // if (MousePickingSystem.HoveredRenderer == ModelRendererZ || CurrentAxisSelected == Axis.Z)
        // {
        // 	ModelRendererZ.Color = Color.WhiteSmoke;
        // }
        // else
        // {
        // 	ModelRendererZ.Color = Color.Cyan;
        // }
    }

    public override void Awake()
    {
        I = this;
        ObjectSelected = false;
        GameObject.UpdateWhenDisabled = true;

        Transform.Pivot = new Vector3(0, 0, 0);

        BoxColliderX = GameObject.AddComponent<BoxShape>();
        BoxColliderX.Size = new Vector3(0.5f, 0.05f, 0.05f);
        BoxColliderX.Offset = new Vector3(-0.01f, 0.01f, -0.01f);
        //boxColliderX.offset = new Vector2(25, 2.5f);

        BoxColliderY = GameObject.AddComponent<BoxShape>();
        BoxColliderY.Size = new Vector3(0.05f, 0.5f, 0.05f);
        BoxColliderY.Offset = new Vector3(-0.01f, 0.01f, -0.01f);

        BoxColliderZ = GameObject.AddComponent<BoxShape>();
        BoxColliderZ.Size = new Vector3(0.05f, 0.05f, 0.5f);
        BoxColliderZ.Offset = new Vector3(-0.01f, 0.01f, -0.01f);

        //boxColliderY.offset = new Vector2(2.5f, 25);

        BoxColliderXy = GameObject.AddComponent<BoxShape>();
        BoxColliderXy.Size = new Vector3(0.1f, 0.1f, 0.1f);
        //boxColliderXY.offset = new Vector3(5, 5,-5)/Units.OneWorldUnit;

        ModelRendererX = GameObject.AddComponent<ModelRendererInstanced>();
        ModelRendererY = GameObject.AddComponent<ModelRendererInstanced>();
        ModelRendererZ = GameObject.AddComponent<ModelRendererInstanced>();
        ModelRendererXy = GameObject.AddComponent<ModelRendererInstanced>();

        // Material unlitMaterial = Tofu.AssetManager.Load<Asset_Material>("ModelRendererUnlit");
        var unlitMaterial = Tofu.AssetManager.Load<Asset_Material>("Assets/Materials/ModelRendererUnlit.mat");
        ModelRendererX.Material = unlitMaterial;
        ModelRendererY.Material = unlitMaterial;
        ModelRendererXy.Material = unlitMaterial;
        ModelRendererZ.Material = unlitMaterial;

        PremadeComponentSetupsHelper.PrepareCube(ModelRendererX);
        PremadeComponentSetupsHelper.PrepareCube(ModelRendererY);
        PremadeComponentSetupsHelper.PrepareCube(ModelRendererXy);
        PremadeComponentSetupsHelper.PrepareCube(ModelRendererZ);

        ModelRendererXy.Layer = 1000;
        ModelRendererX.Layer = 1000;
        ModelRendererY.Layer = 1000;
        ModelRendererZ.Layer = 1000;

        ModelRendererX.BoxShape = BoxColliderX;
        ModelRendererXy.BoxShape = BoxColliderXy;
        ModelRendererY.BoxShape = BoxColliderY;
        ModelRendererZ.BoxShape = BoxColliderZ;


        ModelRendererX.Color = Color.Red;
        ModelRendererY.Color = Color.YellowGreen;
        ModelRendererXy.Color = Color.Gold;
        ModelRendererZ.Color = Color.Cyan;

        ModelRendererX.AutomaticallyFindBoxShape = false;
        ModelRendererY.AutomaticallyFindBoxShape = false;
        ModelRendererZ.AutomaticallyFindBoxShape = false;
        ModelRendererXy.AutomaticallyFindBoxShape = false;

        base.Awake();
    }

    private void SetSelectedObjectRigidbodyAwake(bool tgl)
    {
        // if (selectedTransform?.HasComponent<Rigidbody>() == true & selectedTransform?.GetComponent<Rigidbody>().body?.Awake == false)
        // {
        // 	selectedTransform.GetComponent<Rigidbody>().body.Awake = tgl;
        // }
    }

    public void Move(Vector3 deltaVector)
    {
        // return;
        deltaVector = Camera.MainCamera.ScreenToWorld(deltaVector) * 100 * 5000;

        var moveVector = Vector3.Zero;
        switch (CurrentAxisSelected)
        {
            case Axis.X:
                moveVector += deltaVector.VectorX();
                break;
            case Axis.Y:
                moveVector += deltaVector.VectorY();
                break;
            case Axis.Z:
                moveVector += new Vector3(deltaVector.Z, 0, deltaVector.X);
                break;
            case Axis.Xy:
                moveVector += deltaVector;
                break;
        }

        Transform.LocalPosition += moveVector;
        // Transform.LocalPosition += moveVector; 

        // _selectedTransform.LocalPosition = _selectedTransform.TranslateWorldToLocal(_selectedTransform.WorldPosition);

        for (var i = 0; i < _selectedTransforms.Count; i++)
        {
            _selectedTransforms[i].LocalPosition +=
                moveVector / (_selectedTransforms[i].Parent?.WorldScale ?? Vector3.One);
        }

        // todo just do the position delta move in transform component for (int i = 0; i < selectedTransform.children.Count; i++) selectedTransform.children[i].position += moveVector;

        // if (selectedTransform.HasComponent<Rigidbody>() && selectedTransform.GetComponent<Rigidbody>().isButton == false)
        // {
        // 	lock (Physics.World)
        // 	{
        // 		Rigidbody rigidbody = selectedTransform.GetComponent<Rigidbody>();
        // 		rigidbody.Velocity = Vector2.Zero;
        // 		if (rigidbody.body != null)
        // 		{
        // 			rigidbody.body.Position = selectedTransform.position;
        // 		}
        // 	}
        // }

        if (KeyboardInput.IsKeyDown(Keys.LeftShift))
        {
            for (var i = 0; i < _selectedTransforms.Count; i++)
            {
                switch (CurrentAxisSelected)
                {
                    case Axis.X:
                        _selectedTransforms[i].LocalPosition = new Vector3(
                            Tofu.MouseInput.WorldPosition.TranslateToGrid().X, _selectedTransforms[i].LocalPosition.Y,
                            0);
                        break;
                    case Axis.Y:
                        _selectedTransforms[i].LocalPosition = new Vector3(_selectedTransforms[i].LocalPosition.X,
                            Tofu.MouseInput.WorldPosition.TranslateToGrid().Y, 0);
                        break;
                    case Axis.Xy:
                        _selectedTransforms[i].LocalPosition = Tofu.MouseInput.WorldPosition.TranslateToGrid(50);
                        break;
                }
            }
        }
    }

    public void SelectObjects(List<int> selection)
    {
        // GameObject.SetActive(selection != null);
        GameObject.SetActive(false);
        Transform.MockIsInCanvas = false;

        if (selection == null)
        {
            ObjectSelected = false;
            return;
        }

        if (selection.Exists(i => i == -1))
        {
            ObjectSelected = false;
            return;
        }

        _selectedTransforms = new List<Transform>();
        for (var i = 0; i < selection.Count; i++)
        {
            var go = Tofu.SceneManager.CurrentScene.GetGameObject(selection[i]);

            if (go != null)
            {
                if (go.Transform.IsInCanvas)
                {
                    Transform.MockIsInCanvas = true;
                }

                _selectedTransforms.Add(go.Transform);
            }
        }

        Transform.WorldPosition = GetCenterOfSelection();
        ObjectSelected = _selectedTransforms.Count > 0;
    }

    private Vector3 GetCenterOfSelection()
    {
        var accumulatedPos = Vector3.Zero;
        for (var i = 0; i < _selectedTransforms.Count; i++)
        {
            accumulatedPos += _selectedTransforms[i].WorldPosition;
        }

        accumulatedPos = accumulatedPos / _selectedTransforms.Count;
        return accumulatedPos;
    }

    private Vector3 GetRotationOfSelection()
    {
        var rotation = Vector3.Zero;
        if (_selectedTransforms.Count > 0)
        {
            rotation = _selectedTransforms[0].Rotation;
        }

        return rotation;
    }
}