namespace Tofu3D;

[ExecuteInEditMode]
public class TransformHandle : Component
{
	public enum Axis
	{
		X,
		Y,
		Xy
	}

	public static bool ObjectSelected;
	public BoxShape BoxColliderX;
	public BoxShape BoxColliderXy;
	public BoxShape BoxColliderY;
	//public BoxShape boxColliderZ;
	public ModelRenderer ModelRendererX;
	public ModelRenderer ModelRendererXy;
	public ModelRenderer ModelRendererY;
	//public ModelRenderer boxRendererZ;

	public bool Clicked;
	public Axis? CurrentAxisSelected;
	Transform _selectedTransform;
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

		// boxColliderZ = gameObject.AddComponent<BoxShape>();
		// boxColliderZ.size = new Vector3(5, 5, 50)/Units.OneWorldUnit;
		//boxColliderY.offset = new Vector2(2.5f, 25);

		BoxColliderXy = GameObject.AddComponent<BoxShape>();
		BoxColliderXy.Size = new Vector3(10, 10, 10) / Units.OneWorldUnit;
		//boxColliderXY.offset = new Vector3(5, 5,-5)/Units.OneWorldUnit;

		ModelRendererX = GameObject.AddComponent<ModelRenderer>();
		ModelRendererY = GameObject.AddComponent<ModelRenderer>();
		// boxRendererZ = gameObject.AddComponent<ModelRenderer>();
		ModelRendererXy = GameObject.AddComponent<ModelRenderer>();


		PremadeComponentSetups.PrepareCube(ModelRendererX);
		PremadeComponentSetups.PrepareCube(ModelRendererY);
		PremadeComponentSetups.PrepareCube(ModelRendererXy);
		// PremadeComponentSetups.PrepareCube(boxRendererZ);

		ModelRendererXy.Layer = 1000;
		ModelRendererX.Layer = 1000;
		ModelRendererY.Layer = 1000;
		// boxRendererZ.Layer = 1000;

		ModelRendererX.BoxShape = BoxColliderX;
		ModelRendererXy.BoxShape = BoxColliderXy;
		ModelRendererY.BoxShape = BoxColliderY;
		// boxRendererZ.boxShape = boxColliderZ;

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
		if (Camera.I.IsOrthographic)
		{
			Transform.LocalScale = Vector3.One * Global.EditorScale * Camera.I.OrthographicSize * 1.5f;
		}
		else
		{
			Transform.LocalScale = Vector3.One * Vector3.Distance(Transform.WorldPosition, Camera.I.Transform.WorldPosition) * 0.3f;
		}

		if (MouseInput.ButtonReleased())
		{
			CurrentAxisSelected = null;
			Clicked = false;
		}

		if (MouseInput.ButtonPressed())
		{
			Clicked = false;
			if (MouseInput.WorldPosition.In(BoxColliderX))
			{
				CurrentAxisSelected = Axis.X;
				Clicked = true;
			}

			if (MouseInput.WorldPosition.In(BoxColliderY))
			{
				CurrentAxisSelected = Axis.Y;
				Clicked = true;
			}

			if (MouseInput.WorldPosition.In(BoxColliderXy))
			{
				CurrentAxisSelected = Axis.Xy;
				Clicked = true;
			}
		}

		if (MouseInput.IsButtonDown() && GameObject.ActiveInHierarchy && Clicked)
		{
			SetSelectedObjectRigidbodyAwake(false);
			Move(MouseInput.WorldDelta);
		}
		else
		{
			SetSelectedObjectRigidbodyAwake(true);
		}

		if (ObjectSelected == false || _selectedTransform == null)
			//GameObject.Active = false;
		{
			return;
		}

		Transform.WorldPosition = _selectedTransform.WorldPosition;
		if (MouseInput.WorldPosition.In(BoxColliderX) || CurrentAxisSelected == Axis.X)
		{
			ModelRendererX.Color = Color.WhiteSmoke;
		}
		else
		{
			ModelRendererX.Color = Color.Red;
		}

		if (MouseInput.WorldPosition.In(BoxColliderY) || CurrentAxisSelected == Axis.Y)
		{
			ModelRendererY.Color = Color.WhiteSmoke;
		}
		else
		{
			ModelRendererY.Color = Color.Cyan;
		}

		if (MouseInput.WorldPosition.In(BoxColliderXy) || CurrentAxisSelected == Axis.Xy)
		{
			ModelRendererXy.Color = Color.WhiteSmoke;
		}
		else
		{
			ModelRendererXy.Color = Color.Gold;
		}

		base.Update();
	}

	public void Move(Vector3 deltaVector)
	{
		Vector3 moveVector = Vector3.Zero;
		switch (CurrentAxisSelected)
		{
			case Axis.X:
				moveVector += deltaVector.VectorX();
				break;
			case Axis.Y:
				moveVector += deltaVector.VectorY();
				break;
			case Axis.Xy:
				moveVector += deltaVector;
				break;
		}


		Transform.WorldPosition += moveVector; // we will grab it with offset, soe we want to move it only by change of mouse position
		// _selectedTransform.LocalPosition = _selectedTransform.TranslateWorldToLocal(_selectedTransform.WorldPosition);
		_selectedTransform.LocalPosition += moveVector / (_selectedTransform.Parent?.WorldScale ?? Vector3.One);

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
			switch (CurrentAxisSelected)
			{
				case Axis.X:
					_selectedTransform.LocalPosition = new Vector3(MouseInput.WorldPosition.TranslateToGrid().X, _selectedTransform.LocalPosition.Y, 0);
					break;
				case Axis.Y:
					_selectedTransform.LocalPosition = new Vector3(_selectedTransform.LocalPosition.X, MouseInput.WorldPosition.TranslateToGrid().Y, 0);
					break;
				case Axis.Xy:
					_selectedTransform.LocalPosition = MouseInput.WorldPosition.TranslateToGrid(50);
					break;
			}
		}
	}

	public void SelectObject(GameObject selectedGo)
	{
		GameObject.ActiveSelf = selectedGo != null;

		if (selectedGo == null)
		{
			ObjectSelected = false;
			return;
		}

		Transform.WorldPosition = selectedGo.Transform.WorldPosition;
		_selectedTransform = selectedGo.Transform;
		ObjectSelected = true;
	}
}