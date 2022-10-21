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
	public ModelRenderer BoxRendererX;
	public ModelRenderer BoxRendererXy;
	public ModelRenderer BoxRendererY;
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

		if (GetComponents<BoxRenderer>().Count > 2)
		{
			BoxColliderXy = GetComponent<BoxShape>(0);
			BoxColliderX = GetComponent<BoxShape>(1);
			BoxColliderY = GetComponent<BoxShape>(2);
			//boxColliderZ = GetComponent<BoxShape>(3);

			BoxRendererXy = GetComponent<ModelRenderer>(0);
			BoxRendererX = GetComponent<ModelRenderer>(1);
			BoxRendererY = GetComponent<ModelRenderer>(2);
			//boxRendererZ = GetComponent<ModelRenderer>(3);
		}
		else
		{
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

			BoxRendererX = GameObject.AddComponent<ModelRenderer>();
			BoxRendererY = GameObject.AddComponent<ModelRenderer>();
			// boxRendererZ = gameObject.AddComponent<ModelRenderer>();
			BoxRendererXy = GameObject.AddComponent<ModelRenderer>();


			PremadeComponentSetups.PrepareCube(BoxRendererX);
			PremadeComponentSetups.PrepareCube(BoxRendererY);
			PremadeComponentSetups.PrepareCube(BoxRendererXy);
			// PremadeComponentSetups.PrepareCube(boxRendererZ);

			BoxRendererXy.Layer = 1000;
			BoxRendererX.Layer = 1000;
			BoxRendererY.Layer = 1000;
			// boxRendererZ.Layer = 1000;

			BoxRendererX.BoxShape = BoxColliderX;
			BoxRendererXy.BoxShape = BoxColliderXy;
			BoxRendererY.BoxShape = BoxColliderY;
			// boxRendererZ.boxShape = boxColliderZ;
		}

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
			Transform.Scale = Vector3.One * Global.EditorScale * Camera.I.OrthographicSize * 1.5f;
		}
		else
		{
			Transform.Scale = Vector3.One * Vector3.Distance(Transform.Position, Camera.I.Transform.Position) * 0.3f;
		}

		if (MouseInput.ButtonReleased())
		{
			CurrentAxisSelected = null;
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

		Transform.Position = _selectedTransform.Position;
		if (MouseInput.WorldPosition.In(BoxColliderX) || CurrentAxisSelected == Axis.X)
		{
			BoxRendererX.Color = Color.WhiteSmoke;
		}
		else
		{
			BoxRendererX.Color = Color.Red;
		}

		if (MouseInput.WorldPosition.In(BoxColliderY) || CurrentAxisSelected == Axis.Y)
		{
			BoxRendererY.Color = Color.WhiteSmoke;
		}
		else
		{
			BoxRendererY.Color = Color.Cyan;
		}

		if (MouseInput.WorldPosition.In(BoxColliderXy) || CurrentAxisSelected == Axis.Xy)
		{
			BoxRendererXy.Color = Color.WhiteSmoke;
		}
		else
		{
			BoxRendererXy.Color = Color.Gold;
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

		Transform.Position += moveVector; // we will grab it with offset, soe we want to move it only by change of mouse position
		_selectedTransform.Position = Transform.Position;

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
					_selectedTransform.Position = new Vector3(MouseInput.WorldPosition.TranslateToGrid().X, _selectedTransform.Position.Y, 0);
					break;
				case Axis.Y:
					_selectedTransform.Position = new Vector3(_selectedTransform.Position.X, MouseInput.WorldPosition.TranslateToGrid().Y, 0);
					break;
				case Axis.Xy:
					_selectedTransform.Position = MouseInput.WorldPosition.TranslateToGrid(50);
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

		Transform.Position = selectedGo.Transform.Position;
		_selectedTransform = selectedGo.Transform;
		ObjectSelected = true;
	}
}