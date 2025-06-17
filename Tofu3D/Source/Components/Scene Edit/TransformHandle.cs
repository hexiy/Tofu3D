namespace TofuEngine;

[ExecuteInEditMode]
public partial class TransformHandle : Component, IComponentUpdateable
{
    private List<Transform> _selectedTransforms = new List<Transform>();
    public BoxShape BoxColliderX;
    public BoxShape BoxColliderXy;
    public BoxShape BoxColliderY;
    public BoxShape BoxColliderZ;

    [XmlIgnore]
    public bool Interacting { get; private set; }

    private TransformHandleMode _mode = TransformHandleMode.Position;

    public TransformHandleAxis? CurrentAxisSelected;
    public ModelRenderer ModelRendererX;
    public ModelRenderer ModelRendererXy;
    public ModelRenderer ModelRendererY;

    public ModelRenderer ModelRendererZ;

// create children gameobjects for position handle, rotation handle and scale handle
    public bool ObjectSelected;
    public static TransformHandle I { get; private set; }

    public void Update()
    {
        if (Tofu.MouseInput.IsMouseInView)
        {
            HandleModeChanges();
        }

        if (Camera.CurrentlyRenderingCamera.IsOrthographic)
        {
            Transform.LocalScale = Vector3.One * Camera.CurrentlyRenderingCamera.OrthographicSize * 1.5f;
        }
        else
        {
            Transform.LocalScale = Vector3.One *
                                   Vector3.Distance(Transform.WorldPosition,
                                       Camera.CurrentlyRenderingCamera.Transform.WorldPosition) * 0.2f;
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
                CurrentAxisSelected = TransformHandleAxis.X;
                hoveringAny = true;
            }

            if (MousePickingSystem.HoveredRenderer == ModelRendererY)
            {
                CurrentAxisSelected = TransformHandleAxis.Y;
                hoveringAny = true;
            }

            if (MousePickingSystem.HoveredRenderer == ModelRendererZ)
            {
                CurrentAxisSelected = TransformHandleAxis.Z;
                hoveringAny = true;
            }

            if (MousePickingSystem.HoveredRenderer == ModelRendererXy)
            {
                CurrentAxisSelected = TransformHandleAxis.Xy;
                hoveringAny = true;
            }
        }

        if (hoveringAny)
        {
            Debug.Log("Hovering transformhandle");
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

        if (MousePickingSystem.HoveredRenderer == ModelRendererX || CurrentAxisSelected == TransformHandleAxis.X)
        {
            ModelRendererX.Material.AlbedoColor = Color.WhiteSmoke;
        }
        else
        {
            ModelRendererX.Material.AlbedoColor = Color.Red;
        }

        if (MousePickingSystem.HoveredRenderer == ModelRendererY || CurrentAxisSelected == TransformHandleAxis.Y)
        {
            ModelRendererY.Material.AlbedoColor = Color.WhiteSmoke;
        }
        else
        {
            ModelRendererY.Material.AlbedoColor = Color.YellowGreen;
        }

        if (MousePickingSystem.HoveredRenderer == ModelRendererXy || CurrentAxisSelected == TransformHandleAxis.Xy)
        {
            ModelRendererXy.Material.AlbedoColor = Color.WhiteSmoke;
        }
        else
        {
            ModelRendererXy.Material.AlbedoColor = Color.Gold;
        }

        if (MousePickingSystem.HoveredRenderer == ModelRendererZ || CurrentAxisSelected == TransformHandleAxis.Z)
        {
            ModelRendererZ.Material.AlbedoColor = Color.WhiteSmoke;
        }
        else
        {
            ModelRendererZ.Material.AlbedoColor = Color.Cyan;
        }
    }

    private void HandleModeChanges()
    {
        if (KeyboardInput.IsKeyDown(Keys.W))
        {
            _mode = TransformHandleMode.Position;
        }

        if (KeyboardInput.IsKeyDown(Keys.E))
        {
            _mode = TransformHandleMode.Rotation;
        }

        if (KeyboardInput.IsKeyDown(Keys.R))
        {
            _mode = TransformHandleMode.Scale;
        }
    }

    public override void Awake()
    {
        I = this;

        GameObjectSelectionManager.GameObjectsSelected += OnSelectedObjects;
        ObjectSelected = false;
        GameObject.UpdateWhenDisabled = true;


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

        ModelRendererX = GameObject.AddComponent<ModelRenderer>();
        ModelRendererY = GameObject.AddComponent<ModelRenderer>();
        ModelRendererZ = GameObject.AddComponent<ModelRenderer>();
        ModelRendererXy = GameObject.AddComponent<ModelRenderer>();

        // Material unlitMaterial = Tofu.AssetManager.Load<Asset_Material>("ModelRendererUnlit");
        Asset_Material standardMaterial =
            Tofu.AssetLoadManager.GetDefaultModelRendererInstancedMaterial();


        Asset_Material materialCopy = Tofu.AssetLoadManager.CreateCopyFile(standardMaterial);
        materialCopy.SpecularSmoothness = 0;
        materialCopy.MetallicTextureStrength = 0;
        materialCopy.Smoothness = 0;
        materialCopy.MaterialType = MaterialType.Unlit;
        materialCopy.RenderMode = RenderMode.Transparent;
        materialCopy.IgnoreDepth = true;
        ModelRendererX.Material = Tofu.AssetLoadManager.CreateCopyFile(materialCopy);
        ModelRendererY.Material = Tofu.AssetLoadManager.CreateCopyFile(materialCopy);
        ModelRendererXy.Material = Tofu.AssetLoadManager.CreateCopyFile(materialCopy);
        ModelRendererZ.Material = Tofu.AssetLoadManager.CreateCopyFile(materialCopy);

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


        ModelRendererX.Material.AlbedoColor = Color.Red;
        ModelRendererY.Material.AlbedoColor = Color.YellowGreen;
        ModelRendererXy.Material.AlbedoColor = Color.Gold;
        ModelRendererZ.Material.AlbedoColor = Color.Cyan;

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
        deltaVector = Camera.ActivelyInteractedWithCamera.ScreenToWorld(deltaVector) * 100;

        Vector3 axisDirection = Vector3.Zero;
        Vector3 moveVector = Vector3.Zero;

        switch (CurrentAxisSelected)
        {
            case TransformHandleAxis.X:
                axisDirection = Transform.TransformVectorToWorldSpaceVector(Vector3.Right);
                break;
            case TransformHandleAxis.Y:
                axisDirection = Transform.TransformVectorToWorldSpaceVector(Vector3.Up);
                break;
            case TransformHandleAxis.Z:
                axisDirection = Transform.TransformVectorToWorldSpaceVector(Vector3.Forward);
                break;
        }

        // Vector3 deltaVectorInWorld
        float similarityInDirection = Vector3.Dot(deltaVector.Normalized(), axisDirection);

        // todo,
        if (deltaVector.MaxVectorMember() > 0.01f)
        {
            // Debug.Log("similarityInDirection:" + similarityInDirection);
        }

        switch (CurrentAxisSelected)
        {
            case TransformHandleAxis.X:
                moveVector += deltaVector.VectorX().Abs() * similarityInDirection;
                break;
            case TransformHandleAxis.Y:
                moveVector += deltaVector.VectorY().Abs() * similarityInDirection;
                break;
            case TransformHandleAxis.Z:
                moveVector -= new Vector3(deltaVector.Z, 0, deltaVector.X);
                break;
            case TransformHandleAxis.Xy:
                moveVector +=
                    Camera.ActivelyInteractedWithCamera.Transform.TransformVectorToWorldSpaceVector(deltaVector);
                break;
        }

        Transform.LocalPosition += moveVector;
        // Transform.LocalPosition += moveVector; 

        // _selectedTransform.LocalPosition = _selectedTransform.TranslateWorldToLocal(_selectedTransform.WorldPosition);

        for (int i = 0; i < _selectedTransforms.Count; i++)
        {
            // _selectedTransforms[i].LocalPosition +=
            // moveVector / (_selectedTransforms[i].Parent?.WorldScale ?? Vector3.One);

            _selectedTransforms[i].LocalPosition +=
                moveVector;
        }

        // Debug.Log($"Moving by:{moveVector}");

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
            for (int i = 0; i < _selectedTransforms.Count; i++)
            {
                switch (CurrentAxisSelected)
                {
                    case TransformHandleAxis.X:
                        _selectedTransforms[i].LocalPosition = new Vector3(
                            Tofu.MouseInput.WorldPosition.TranslateToGrid().X, _selectedTransforms[i].LocalPosition.Y,
                            0);
                        break;
                    case TransformHandleAxis.Y:
                        _selectedTransforms[i].LocalPosition = new Vector3(_selectedTransforms[i].LocalPosition.X,
                            Tofu.MouseInput.WorldPosition.TranslateToGrid().Y, 0);
                        break;
                    case TransformHandleAxis.Xy:
                        _selectedTransforms[i].LocalPosition = Tofu.MouseInput.WorldPosition.TranslateToGrid(50);
                        break;
                }
            }
        }
    }

    private void OnSelectedObjects(List<GameObject> selection)
    {
        // GameObject.SetActive(selection != null);
        // GameObject.SetActive(false);
        // Transform.MockIsInCanvas = false;

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
        for (int i = 0; i < selection.Count; i++)
        {
            // var go = Tofu.SceneManager.CurrentScene.GetGameObjectByID(selection[i]);
            GameObject? go = selection[i];

            if (go != null)
            {
                // if (go.Transform.IsInCanvas)
                // {
                //     Transform.MockIsInCanvas = true;
                // }

                _selectedTransforms.Add(go.Transform);
            }
        }

        Transform.WorldPosition = GetCenterOfSelection();

        ObjectSelected = _selectedTransforms.Count > 0;
    }

    private Vector3 GetCenterOfSelection()
    {
        Vector3 accumulatedPos = Vector3.Zero;
        for (int i = 0; i < _selectedTransforms.Count; i++)
        {
            accumulatedPos += _selectedTransforms[i].WorldPosition;
        }

        accumulatedPos = accumulatedPos / _selectedTransforms.Count;
        return accumulatedPos;
    }

    private Vector3 GetRotationOfSelection()
    {
        Vector3 rotation = Vector3.Zero;
        if (_selectedTransforms.Count > 0)
        {
            rotation = _selectedTransforms[0].Rotation;
        }

        return rotation;
    }
}