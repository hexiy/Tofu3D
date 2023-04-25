namespace Tofu3D;

[ExecuteInEditMode]
public class TransformHandle : Component
{
	public enum Axis
	{
		X,
		Y,
		Z,
		Xy,
	}

	public static bool ObjectSelected;
	public BoxShape BoxColliderX;
	public BoxShape BoxColliderXy;
	public BoxShape BoxColliderY;
	public BoxShape BoxColliderZ;
	public ModelRenderer ModelRendererX;
	public ModelRenderer ModelRendererXy;
	public ModelRenderer ModelRendererY;
	public ModelRenderer ModelRendererZ;

	public bool Clicked;
	public Axis? CurrentAxisSelected;
	List<Transform> _selectedTransforms;
	public static TransformHandle I { get; private set; }

	public override void Awake()
	{
		I = this;
		ObjectSelected = false;
		GameObject.UpdateWhenDisabled = true;

		BoxColliderX = GameObject.AddComponent<BoxShape>();
		BoxColliderX.Size = new Vector3(50, 5, 5) / Units.OneWorldUnit;
		//boxColliderX.offset = new Vector2(25, 2.5f);

		BoxColliderY = GameObject.AddComponent<BoxShape>();
		BoxColliderY.Size = new Vector3(5, 50, 5) / Units.OneWorldUnit;

		BoxColliderZ = GameObject.AddComponent<BoxShape>();
		BoxColliderZ.Size = new Vector3(5, 5, 50) / Units.OneWorldUnit;
		//boxColliderY.offset = new Vector2(2.5f, 25);

		BoxColliderXy = GameObject.AddComponent<BoxShape>();
		BoxColliderXy.Size = new Vector3(10, 10, 10) / Units.OneWorldUnit;
		//boxColliderXY.offset = new Vector3(5, 5,-5)/Units.OneWorldUnit;

		ModelRendererX = GameObject.AddComponent<ModelRenderer>();
		ModelRendererY = GameObject.AddComponent<ModelRenderer>();
		ModelRendererZ = GameObject.AddComponent<ModelRenderer>();
		ModelRendererXy = GameObject.AddComponent<ModelRenderer>();

		Material unlitMaterial = AssetManager.Load<Material>("ModelRendererUnlit");
		ModelRendererX.Material = unlitMaterial;
		ModelRendererY.Material = unlitMaterial;
		ModelRendererXy.Material = unlitMaterial;
		ModelRendererZ.Material = unlitMaterial;

		PremadeComponentSetups.PrepareCube(ModelRendererX);
		PremadeComponentSetups.PrepareCube(ModelRendererY);
		PremadeComponentSetups.PrepareCube(ModelRendererXy);
		PremadeComponentSetups.PrepareCube(ModelRendererZ);

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


		base.Awake();
	}

	void SetSelectedObjectRigidbodyAwake(bool tgl)
	{
		// if (selectedTransform?.HasComponent<Rigidbody>() == true & selectedTransform?.GetComponent<Rigidbody>().body?.Awake == false)
		// {
		// 	selectedTransform.GetComponent<Rigidbody>().body.Awake = tgl;
		// }
	}

	public override void Update()
	{
		if (Camera.MainCamera.IsOrthographic)
		{
			Transform.LocalScale = Vector3.One * Camera.MainCamera.OrthographicSize * 1.5f;
		}
		else
		{
			Transform.LocalScale = Vector3.One * Vector3.Distance(Transform.WorldPosition, Camera.MainCamera.Transform.WorldPosition) * 0.3f;
		}

		if (MouseInput.ButtonReleased())
		{
			CurrentAxisSelected = null;
			Clicked = false;
		}

		if (MouseInput.ButtonPressed())
		{
			Clicked = false;
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
		}

		if (MouseInput.IsButtonDown() && GameObject.ActiveInHierarchy && Clicked)
		{
			SetSelectedObjectRigidbodyAwake(false);
			// Move(MouseInput.WorldDelta);
			Move(MouseInput.ScreenDelta / Units.OneWorldUnit); // /_selectedTransforms[0].GetComponent<Renderer>().DistanceFromCamera * 1000f);
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

		base.Update();
	}

	public void Move(Vector3 deltaVector)
	{
		// return;
		deltaVector = Camera.MainCamera.ScreenToWorld(deltaVector) * 100 * 5000;

		Vector3 moveVector = Vector3.Zero;
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

		for (int i = 0; i < _selectedTransforms.Count; i++)
		{
			_selectedTransforms[i].LocalPosition += moveVector / (_selectedTransforms[i].Parent?.WorldScale ?? Vector3.One);
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
			for (int i = 0; i < _selectedTransforms.Count; i++)
			{
				switch (CurrentAxisSelected)
				{
					case Axis.X:
						_selectedTransforms[i].LocalPosition = new Vector3(MouseInput.WorldPosition.TranslateToGrid().X, _selectedTransforms[i].LocalPosition.Y, 0);
						break;
					case Axis.Y:
						_selectedTransforms[i].LocalPosition = new Vector3(_selectedTransforms[i].LocalPosition.X, MouseInput.WorldPosition.TranslateToGrid().Y, 0);
						break;
					case Axis.Xy:
						_selectedTransforms[i].LocalPosition = MouseInput.WorldPosition.TranslateToGrid(50);
						break;
				}
			}
		}
	}

	public void SelectObjects(List<int> selection)
	{
		GameObject.SetActive(selection != null);

		if (selection == null)
		{
			ObjectSelected = false;
			return;
		}

		_selectedTransforms = new List<Transform>();
		for (int i = 0; i < selection.Count; i++)
		{
			GameObject go = SceneManager.CurrentScene.GetGameObject(selection[i]);
			if (go != null)
			{
				_selectedTransforms.Add(go.Transform);
			}
		}

		Transform.WorldPosition = GetCenterOfSelection();
		ObjectSelected = true;
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

	Vector3 GetRotationOfSelection()
	{
		Vector3 rotation = Vector3.Zero;
		if (_selectedTransforms.Count > 0)
		{
			rotation = _selectedTransforms[0].Rotation;
		}

		return rotation;
	}
}