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

    private List<Transform> _selectedTransforms = new List<Transform>();
    public BoxShape BoxColliderX;
    public BoxShape BoxColliderXy;
    public BoxShape BoxColliderY;
    public BoxShape BoxColliderZ;

    [XmlIgnore]
    public bool Interacting { get; private set; }

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

        if (Tofu.MouseInput.IsButtonUp())
        {
            CurrentAxisSelected = null;
        }

        bool hoveringAny = false;
        if (Interacting == false)
        {
            if (MousePickingSystem.HoveredRenderer == ModelRendererX)
            {
                CurrentAxisSelected = Axis.X;
                hoveringAny = true;
            }

            if (MousePickingSystem.HoveredRenderer == ModelRendererY)
            {
                CurrentAxisSelected = Axis.Y;
                hoveringAny = true;
            }

            if (MousePickingSystem.HoveredRenderer == ModelRendererZ)
            {
                CurrentAxisSelected = Axis.Z;
                hoveringAny = true;
            }

            if (MousePickingSystem.HoveredRenderer == ModelRendererXy)
            {
                CurrentAxisSelected = Axis.Xy;
                hoveringAny = true;
            }
        }

        if (hoveringAny && Tofu.MouseInput.ButtonPressed())
        {
            Interacting = true;
        }

        if (hoveringAny == false && Tofu.MouseInput.IsButtonUp())
        {
            Interacting = false;
        }

        if (Tofu.MouseInput.IsButtonDown() && GameObject.ActiveInHierarchy && Interacting)
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
        if (Interacting)
        {
            return;
        }

        if (MousePickingSystem.HoveredRenderer == ModelRendererX || CurrentAxisSelected == Axis.X)
        {
            ModelRendererX.Material.AlbedoTint = Color.WhiteSmoke;
        }
        else
        {
            ModelRendererX.Material.AlbedoTint = Color.Red;
        }

        if (MousePickingSystem.HoveredRenderer == ModelRendererY || CurrentAxisSelected == Axis.Y)
        {
            ModelRendererY.Material.AlbedoTint = Color.WhiteSmoke;
        }
        else
        {
            ModelRendererY.Material.AlbedoTint = Color.YellowGreen;
        }

        if (MousePickingSystem.HoveredRenderer == ModelRendererXy || CurrentAxisSelected == Axis.Xy)
        {
            ModelRendererXy.Material.AlbedoTint = Color.WhiteSmoke;
        }
        else
        {
            ModelRendererXy.Material.AlbedoTint = Color.Gold;
        }

        if (MousePickingSystem.HoveredRenderer == ModelRendererZ || CurrentAxisSelected == Axis.Z)
        {
            ModelRendererZ.Material.AlbedoTint = Color.WhiteSmoke;
        }
        else
        {
            ModelRendererZ.Material.AlbedoTint = Color.Cyan;
        }
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
        Asset_Material material =
            Tofu.AssetLoadManager.Load<Asset_Material>("Assets/Materials/ModelRendererInstanced.mat");

        Asset_Material materialCopy = material.CreateRuntimeCopy();
        materialCopy.SpecularSmoothness = 0;
        materialCopy.MetallicTextureStrength = 0;
        materialCopy.Smoothness = 0;
        ModelRendererX.Material = materialCopy.CreateRuntimeCopy();
        ModelRendererY.Material = materialCopy.CreateRuntimeCopy();
        ModelRendererXy.Material = materialCopy.CreateRuntimeCopy();
        ModelRendererZ.Material = materialCopy.CreateRuntimeCopy();

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


        ModelRendererX.Material.AlbedoTint = Color.Red;
        ModelRendererY.Material.AlbedoTint = Color.YellowGreen;
        ModelRendererXy.Material.AlbedoTint = Color.Gold;
        ModelRendererZ.Material.AlbedoTint = Color.Cyan;

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
        deltaVector = Camera.MainCamera.ScreenToWorld(deltaVector) * 100;

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
                moveVector -= new Vector3(deltaVector.Z, 0, deltaVector.X);
                break;
            case Axis.Xy:
                moveVector += Camera.MainCamera.Transform.TransformVectorToWorldSpaceVector(deltaVector);
                break;
        }

        Transform.LocalPosition += moveVector;
        // Transform.LocalPosition += moveVector; 

        // _selectedTransform.LocalPosition = _selectedTransform.TranslateWorldToLocal(_selectedTransform.WorldPosition);

        for (var i = 0; i < _selectedTransforms.Count; i++)
        {
            // _selectedTransforms[i].LocalPosition +=
            // moveVector / (_selectedTransforms[i].Parent?.WorldScale ?? Vector3.One);

            _selectedTransforms[i].LocalPosition +=
                moveVector;
        }

        Debug.Log($"Moving by:{moveVector}");

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

    public void SelectObjects(List<GameObject> selection)
    {
        // GameObject.SetActive(selection != null);
        // GameObject.SetActive(false);
        Transform.MockIsInCanvas = false;

        if (selection == null)
        {
            ObjectSelected = false;
            return;
        }

        // if (selection.Exists(i => i == -1))
        // {
            // ObjectSelected = false;
            // return;
        // }

        _selectedTransforms = new List<Transform>();
        for (var i = 0; i < selection.Count; i++)
        {
            // var go = Tofu.SceneManager.CurrentScene.GetGameObjectByID(selection[i]);
            var go = selection[i];

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