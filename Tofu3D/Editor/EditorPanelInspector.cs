using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelInspector : EditorPanel
{
	public override Vector2 Size => new Vector2(700, Editor.SceneViewSize.Y);
	public override Vector2 Position => new Vector2(Tofu.I.Window.ClientSize.X - EditorPanelInspector.I.WindowWidth, 0);
	public override Vector2 Pivot => new Vector2(1, 0);

	public override string Name => "Inspector";

	string _addComponentPopupText = "";
	int _contentMaxWidth;

	public static EditorPanelInspector I { get; private set; }

	List<InspectableData> _currentInspectableDatas = new List<InspectableData>(); // cached inspectable data
	bool _editing;

	private bool HasInspectableData
	{
		get { return _currentInspectableDatas.Count > 0; }
	}

	public override void Init()
	{
		I = this;

		_componentTypes = typeof(Component).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract).ToList();
		Scene.ComponentAdded += OnComponentAddedToScene;
		GameObjectSelectionManager.GameObjectsSelected += OnGameObjectsSelected;
		MouseInput.RegisterPassThroughEdgesCondition(() => _editing && MouseInput.IsButtonDown(MouseInput.Buttons.Left));
	}

	void OnComponentAddedToScene(Component comp)
	{
		// if (_selectedInspectable == comp.GameObject)
		// {
		// 	UpdateCurrentComponentsCache();
		// 	return;
		// }
	}

	public override void Update()
	{
	}

	public void ClearInspectableDatas()
	{
		_currentInspectableDatas.Clear();
	}

	private void OnGameObjectsSelected(List<int> ids)
	{
		// if (ids.Count != 1)
		// {
		// 	_selectedInspectable = null;
		// }
		// else
		// {
		// 	_selectedInspectable = SceneManager.CurrentScene.GetGameObject(ids[0]);
		// 	UpdateCurrentComponentsCache();
		// 	_selectedMaterial = null;
		// }

		if (ids.Count == 0)
		{
			ClearInspectableDatas();

			return;
		}


		GameObject go = SceneManager.CurrentScene.GetGameObject(ids[0]);
		SelectInspectables(go.Components.ToList<Tofu3D.IInspectable>());
	}

	/*private void UpdateCurrentComponentsCache()
	{
		// if (_selectedInspectable == null)
		// {
		// 	return;
		// }
		//
		// _currentInspectableDatas.Clear();
		// for (int componentIndex = 0; componentIndex < _selectedInspectable.Components.Count; componentIndex++)
		// {
		// 	InspectableData data = new InspectableData(_selectedInspectable.Components[componentIndex]);
		// 	_currentInspectableDatas.Add(data);
		// }
	}*/
	public void SelectInspectable(IInspectable inspectable)
	{
		SelectInspectables(new List<IInspectable>() {inspectable});
	}

	public void SelectInspectables(List<IInspectable> inspectables)
	{
		ClearInspectableDatas();

		foreach (IInspectable inspectable in inspectables)
		{
			InspectableData inspectableData = new InspectableData(inspectable);
			_currentInspectableDatas.Add(inspectableData);
		}
	}

	public void OnMaterialSelected(string materialPath)
	{
		// _selectedMaterial = MaterialCache.GetMaterial(Path.GetFileName(materialPath)); //MaterialAssetManager.LoadMaterial(materialPath);
		//
		// OnGameObjectsSelected(new List<int>());
	}

	public override void Draw()
	{
		if (Active == false)
		{
			return;
		}

		WindowWidth = 800;
		_contentMaxWidth = WindowWidth - (int) ImGui.GetStyle().WindowPadding.X * 1;
		ImGui.SetNextItemWidth(WindowWidth);
		ImGui.SetNextWindowPos(new Vector2(Tofu.I.Window.ClientSize.X, 0), ImGuiCond.FirstUseEver, new Vector2(1, 0));
		ImGui.Begin(Name, Editor.ImGuiDefaultWindowFlags | ImGuiWindowFlags.NoScrollbar);

		ImGui.SameLine();

		bool showDebugButton = KeyboardInput.IsKeyDown(Keys.LeftSuper);
		if (showDebugButton)
		{
			bool debugButtonClicked = ImGui.SmallButton($"Debug [{(Global.Debug ? "ON" : "OFF")}]");
			if (debugButtonClicked)
			{
				Global.Debug = !Global.Debug;
			}
		}

		ImGui.Spacing();

		ResetId();


		if (HasInspectableData)
		{
			ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2);
			DrawInspectables();
			ImGui.PopStyleVar(1);


			// properties with ShowIf and ShowIfNot attributes need to be reevaluated to show or not
			// if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
			// {
			// 	UpdateCurrentComponentsCache();
			// }
		}

		// if (_selectedMaterial != null)
		// {
		// 	DrawMaterialInspector();
		// }

		ImGui.End();
	}

	public static List<Type> InspectorSupportedTypes = new()
	                                                   {
		                                                   typeof(GameObject),
		                                                   typeof(Material),
		                                                   typeof(Vector3),
		                                                   typeof(Vector2),
		                                                   typeof(Texture),
		                                                   typeof(Color),
		                                                   typeof(bool),
		                                                   typeof(float),
		                                                   typeof(int),
		                                                   typeof(string),
		                                                   typeof(List<GameObject>),
		                                                   typeof(Action),
		                                                   typeof(AudioClip),
		                                                   typeof(Model),
	                                                   };
	List<Type> _componentTypes;

	void DrawInspectables()
	{
		GameObject gameObject = (_currentInspectableDatas[0].Inspectable as Component)?.GameObject;
		if (gameObject?.IsPrefab == true)
		{
			if (ImGui.Button("Update prefab"))
			{
				SceneSerializer.SaveGameObject(gameObject, gameObject.PrefabPath);
			}

			ImGui.SameLine();
			if (ImGui.Button("Delete prefab"))
			{
				gameObject.IsPrefab = false;
			}
		}

		_editing = false;

		if (gameObject)
		{
			PushNextId();
			ImGui.SetScrollX(0);

			string gameObjectName = gameObject.Name;
			ImGui.Checkbox("", ref gameObject.ActiveSelf);
			ImGui.SameLine();
			PushNextId();
			ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
			if (ImGui.InputText("", ref gameObjectName, 100))
			{
				gameObject.Name = gameObjectName;
			}
		}

		foreach (InspectableData componentInspectorData in _currentInspectableDatas)
		{
			Component component = componentInspectorData.Inspectable as Component;

			if (component)
			{
				PushNextId();

				ImGui.Checkbox("", ref component.Enabled);
				ImGui.SameLine();

				if (ImGui.Button("-"))
				{
					component.GameObject.RemoveComponent(component);
					continue;
				}

				ImGui.SameLine();
			}

			PushNextId();

			if (ImGui.CollapsingHeader(componentInspectorData.InspectableType.Name, ImGuiTreeNodeFlags.DefaultOpen))
			{
				foreach (FieldOrPropertyInfo info in componentInspectorData.Infos)
				{
					if (info.CanShowInEditor == false)
					{
						continue;
					}

					// if (info.GetValue(componentInspectorData.Inspectable) == null)
					// {
					// 	if (info.FieldOrPropertyType.GetType().IsTypeDefinition)
					// 	{
					// 		info.SetValue(componentInspectorData.Inspectable, default(object));
					// 	}
					// 	else
					// 	{
					// 		var instance = Activator.CreateInstance(info.FieldOrPropertyType.GetType());
					// 		info.SetValue(componentInspectorData.Inspectable, instance);
					// 	}
					// }

					PushNextId();

					// ReSharper disable once ReplaceWithSingleAssignment.False
					bool hovering = false;
					if (ImGui.IsMouseHoveringRect(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + new System.Numerics.Vector2(1500, ImGui.GetFrameHeightWithSpacing())))
					{
						hovering = true;
					}

					ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);

					if (info.IsReadonly)
					{
						ImGui.BeginDisabled();
					}

					if (hovering)
					{
						ImGui.TextColored(new Vector4(0.7f, 0.4f, 0.6f, 1), info.Name);
					}
					else
					{
						ImGui.Text(info.Name);
					}

					float itemWidth1 = 400;
					ImGui.SameLine(ImGui.GetWindowWidth() - itemWidth1);
					ImGui.SetNextItemWidth(itemWidth1);

					if (info.FieldOrPropertyType == typeof(Vector3))
					{
						System.Numerics.Vector3 systemv3 = (Vector3) info.GetValue(componentInspectorData.Inspectable);
						if (ImGui.DragFloat3("", ref systemv3, 0.01f))
						{
							info.SetValue(componentInspectorData.Inspectable, (Vector3) systemv3);
						}
					}
					else if (info.FieldOrPropertyType == typeof(Vector2))
					{
						System.Numerics.Vector2 systemv2 = (Vector2) info.GetValue(componentInspectorData.Inspectable);
						if (ImGui.DragFloat2("", ref systemv2, 0.01f))
						{
							info.SetValue(componentInspectorData.Inspectable, (Vector2) systemv2);
						}
					}
					else if (info.FieldOrPropertyType == typeof(Model))
					{
						Model model = (Model) info.GetValue(componentInspectorData.Inspectable);

						string assetName = Path.GetFileName(model?.AssetPath) ?? "";

						bool clicked = ImGui.Button(assetName, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));

						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("CONTENT_BROWSER_MODEL", ImGuiDragDropFlags.None);
							string filePath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && filePath.Length > 0)
							{
								// fileName = Path.GetRelativePath("Assets", fileName);

								model = AssetManager.Load<Model>(filePath);
								// gameObject.GetComponent<Renderer>().Material.Vao = model.Vao; // materials are shared
								info.SetValue(componentInspectorData.Inspectable, model);
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (info.FieldOrPropertyType == typeof(AudioClip))
					{
						AudioClip audioClip = (AudioClip) info.GetValue(componentInspectorData.Inspectable);
						if (audioClip == null)
						{
							audioClip = new AudioClip();
							info.SetValue(componentInspectorData.Inspectable, audioClip);
						}

						string clipName = Path.GetFileName(audioClip?.AssetPath);

						bool clicked = ImGui.Button(clipName, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));


						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("CONTENT_BROWSER_AUDIOCLIP", ImGuiDragDropFlags.None);
							string fileName = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && fileName.Length > 0)
							{
								// fileName = Path.GetRelativePath("Assets", fileName);

								audioClip.AssetPath = fileName;
								info.SetValue(componentInspectorData.Inspectable, audioClip);
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (info.FieldOrPropertyType == typeof(List<GameObject>))
					{
						List<GameObject> listOfGameObjects = (List<GameObject>) info.GetValue(componentInspectorData.Inspectable);
						if (ImGui.Button("+"))
						{
							listOfGameObjects.Add(null);
							info.SetValue(componentInspectorData.Inspectable, listOfGameObjects);
						}

						ImGui.SameLine();
						if (ImGui.CollapsingHeader("List<GameObject>", ImGuiTreeNodeFlags.DefaultOpen))
						{
							for (int j = 0; j < listOfGameObjects.Count; j++)
							{
								ImGui.PushStyleColor(ImGuiCol.TextSelectedBg, Color.Aqua.ToVector4());
								PushNextId();
								bool xClicked = ImGui.Button("x", new System.Numerics.Vector2(ImGui.GetFrameHeight(), ImGui.GetFrameHeight()));

								if (xClicked)
								{
									listOfGameObjects.RemoveAt(j);
									info.SetValue(componentInspectorData.Inspectable, listOfGameObjects);
									continue;
								}

								ImGui.SameLine();

								bool isNull = listOfGameObjects[j] == null;
								string name = isNull ? "<null>" : listOfGameObjects[j].Name;

								bool selectableClicked = ImGui.Selectable(name);
								if (selectableClicked && isNull == false)
								{
									EditorPanelHierarchy.I.SelectGameObject(listOfGameObjects[j].Id);
									return;
								}

								if (ImGui.BeginDragDropTarget())
								{
									ImGui.AcceptDragDropPayload("GAMEOBJECT", ImGuiDragDropFlags.None);

									string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
									ImGuiPayloadPtr x = ImGui.GetDragDropPayload();
									if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
									{
										GameObject foundGo = SceneManager.CurrentScene.GetGameObject(int.Parse(payload));
										listOfGameObjects[j] = foundGo;
										info.SetValue(componentInspectorData.Inspectable, listOfGameObjects);
									}

									ImGui.EndDragDropTarget();
								}

								ImGui.PopStyleColor();
							}
						}
					}
					else if (info.FieldOrPropertyType == typeof(Action))
					{
						Action action = (Action) info.GetValue(componentInspectorData.Inspectable);
						ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int) ImGuiCol.Text]);
						if (ImGui.Button($"> {info.Name} <", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight())))
						{
							action?.Invoke();
						}

						ImGui.PopStyleColor(1);
					}
					else if (info.FieldOrPropertyType == typeof(Texture) && componentInspectorData.Inspectable is TextureRenderer)
					{
						string textureName = Path.GetFileName((componentInspectorData.Inspectable as TextureRenderer).Texture?.AssetPath);

						bool clicked = ImGui.Button(textureName, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
						//ImiGui.Text(textureName);
						if (clicked)
						{
							EditorPanelBrowser.I.GoToFile((componentInspectorData.Inspectable as TextureRenderer).Texture.AssetPath);
						}

						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("CONTENT_BROWSER_TEXTURE", ImGuiDragDropFlags.None);
							string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
							{
								payload = Path.GetRelativePath("Assets", payload);

								textureName = payload;

								(componentInspectorData.Inspectable as TextureRenderer).LoadTexture(textureName);
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (info.FieldOrPropertyType == typeof(Material))
					{
						string materialPath = Path.GetFileName((componentInspectorData.Inspectable as Renderer).Material.FilePath);

						materialPath = materialPath ?? "";
						bool clicked = ImGui.Button(materialPath, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
						if (clicked)
						{
							EditorPanelBrowser.I.GoToFile(materialPath);
						}

						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("CONTENT_BROWSER_MATERIAL", ImGuiDragDropFlags.None);
							string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
							{
								payload = payload;
								string materialName = Path.GetFileName(payload);
								Material draggedMaterial = MaterialAssetManager.LoadMaterial(payload);
								if (draggedMaterial.Shader == null)
								{
									Debug.Log("No Shader attached to material.");
								}
								else
								{
									(componentInspectorData.Inspectable as Renderer).Material = draggedMaterial;
								}
								// load new material
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (info.FieldOrPropertyType == typeof(GameObject))
					{
						GameObject goObject = info.GetValue(componentInspectorData.Inspectable) as GameObject;
						string fieldGoName = goObject?.Name ?? "";
						bool clicked = ImGui.Button(fieldGoName, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
						if (clicked && goObject != null)
						{
							EditorPanelHierarchy.I.SelectGameObject(goObject.Id);
							return;
						}

						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("PREFAB_PATH", ImGuiDragDropFlags.None);
							string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							string dataType = ImGui.GetDragDropPayload().DataType.GetStringASCII().Replace("\0", string.Empty);
							if (dataType == "PREFAB_PATH")
							{
								if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
								{
									GameObject loadedGo = SceneSerializer.LoadPrefab(payload, true);
									info.SetValue(componentInspectorData.Inspectable, loadedGo);
								}
							}

							ImGui.EndDragDropTarget();
						}

						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("GAMEOBJECT", ImGuiDragDropFlags.None);
							string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							string dataType = ImGui.GetDragDropPayload().DataType.GetStringASCII().Replace("\0", string.Empty);

							if (dataType == "GAMEOBJECT")
							{
								//	string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
								if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
								{
									GameObject foundGo = SceneManager.CurrentScene.GetGameObject(int.Parse(payload));
									info.SetValue(componentInspectorData.Inspectable, foundGo);
								}
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (info.FieldOrPropertyType == typeof(Color))
					{
						System.Numerics.Vector4 fieldValue = ((Color) info.GetValue(componentInspectorData.Inspectable)).ToVector4();

						if (ImGui.ColorEdit4("", ref fieldValue))
						{
							info.SetValue(componentInspectorData.Inspectable, fieldValue.ToColor());
						}
					}
					else if (info.FieldOrPropertyType == typeof(bool))
					{
						ImGui.SameLine(ImGui.GetWindowWidth() - ImGui.GetContentRegionAvail().X / 2);

						bool fieldValue = (bool) info.GetValue(componentInspectorData.Inspectable);

						if (ImGui.Checkbox("", ref fieldValue))
						{
							info.SetValue(componentInspectorData.Inspectable, fieldValue);
						}
					}
					else if (info.FieldOrPropertyType == typeof(float))
					{
						float fieldValue = (float) info.GetValue(componentInspectorData.Inspectable);

						SliderF sliderAttrib = null;
						List<CustomAttributeData> a = info.FieldOrPropertyType.CustomAttributes.ToList();
						for (int i = 0; i < info.CustomAttributes.Count(); i++)
						{
							if (info.CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(SliderF))
							{
								FieldInfo fieldType = componentInspectorData.Inspectable.GetType().GetField(info.Name);
								if (fieldType != null)
								{
									sliderAttrib = fieldType.GetCustomAttribute<SliderF>();
								}
								else
								{
									PropertyInfo propertyType = componentInspectorData.Inspectable.GetType().GetProperty(info.Name);
									sliderAttrib = propertyType.GetCustomAttribute<SliderF>();
								}
							}
						}

						if (sliderAttrib != null)
						{
							if (ImGui.SliderFloat("", ref fieldValue, sliderAttrib.MinValue, sliderAttrib.MaxValue))
							{
								info.SetValue(componentInspectorData.Inspectable, fieldValue);
							}
						}
						else
						{
							if (ImGui.DragFloat("", ref fieldValue, 0.01f, float.NegativeInfinity, float.PositiveInfinity, "%.05f"))
							{
								info.SetValue(componentInspectorData.Inspectable, fieldValue);
							}
						}
					}
					else if (info.FieldOrPropertyType == typeof(int))
					{
						int fieldValue = (int) info.GetValue(componentInspectorData.Inspectable);


						if (ImGui.DragInt("", ref fieldValue))
						{
							info.SetValue(componentInspectorData.Inspectable, fieldValue);
						}
					}
					else if (info.FieldOrPropertyType == typeof(string))
					{
						string fieldValue = info.GetValue(componentInspectorData.Inspectable)?.ToString();

						if (ImGui.InputTextMultiline("", ref fieldValue, 100, new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, 200)))
						{
							info.SetValue(componentInspectorData.Inspectable, fieldValue);
						}
					}

					if (info.IsReadonly)
					{
						ImGui.EndDisabled();
					}

					if (ImGui.IsItemEdited())
					{
						_editing = true;
					}
					//ImGui.PopID();
				}
			}
		}

		if (gameObject)
		{
			bool justOpened = false;
			if (ImGui.Button("+"))
			{
				ImGui.OpenPopup("AddComponentPopup");
				justOpened = true;
			}

			if (ImGui.BeginPopupContextWindow("AddComponentPopup"))
			{
				if (justOpened)
				{
					ImGui.SetKeyboardFocusHere(0);
				}

				bool enterPressed = ImGui.InputText("", ref _addComponentPopupText, 100, ImGuiInputTextFlags.EnterReturnsTrue);


				if (_addComponentPopupText.Length > 0)
				{
					for (int i = 0; i < _componentTypes.Count; i++)
					{
						if (_componentTypes[i].Name.ToLower().Contains(_addComponentPopupText.ToLower()))
						{
							if (ImGui.Button(_componentTypes[i].Name) || enterPressed)
							{
								gameObject.AddComponent(_componentTypes[i]);
								ImGui.CloseCurrentPopup();
								break;
							}
						}
					}
				}
				else
				{
					for (int i = 0; i < _componentTypes.Count; i++)
					{
						if (ImGui.Button(_componentTypes[i].Name))
						{
							gameObject.AddComponent(_componentTypes[i]);
							ImGui.CloseCurrentPopup();
						}
					}
				}

				ImGui.EndPopup();
			}
		}
	}
	/*
	void DrawMaterialInspector()
	{
		PushNextId();
		string materialName = Path.GetFileNameWithoutExtension(_selectedMaterial.FilePath);
		ImGui.Text(materialName);

		ImGui.Text("Shader");
		float itemWidth = 200;
		ImGui.SameLine(ImGui.GetWindowWidth() - itemWidth);
		ImGui.SetNextItemWidth(itemWidth);

		string shaderPath = _selectedMaterial.Shader?.Path ?? "";
		string shaderName = Path.GetFileName(shaderPath);
		bool clicked = ImGui.Button(shaderName, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
		if (clicked)
		{
			EditorPanelBrowser.I.GoToFile(shaderPath);
		}

		if (ImGui.BeginDragDropTarget())
		{
			ImGui.AcceptDragDropPayload("SHADER", ImGuiDragDropFlags.None);

			shaderPath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
			if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && shaderPath.Length > 0)
			{
				//shaderPath = Path.GetRelativePath("Assets", shaderPath);
				Shader shader = new(shaderPath);

				_selectedMaterial.Shader = shader;
				MaterialAssetManager.SaveMaterial(_selectedMaterial);
			}

			ImGui.EndDragDropTarget();
		}

		if (_selectedMaterial.Shader != null)
		{
			ShaderUniform[] shaderUniforms = _selectedMaterial.Shader.GetAllUniforms();
			for (int i = 0; i < shaderUniforms.Length; i++)
			{
				PushNextId();

				ImGui.Text(shaderUniforms[i].Name);

				if (shaderUniforms[i].Type == typeof(Vector4))
				{
					ImGui.SameLine(ImGui.GetWindowWidth() - 200 - 5);
					ImGui.SetNextItemWidth(itemWidth);

					if (_selectedMaterial.Shader.Uniforms.ContainsKey(shaderUniforms[i].Name) == false)
					{
						continue;
					}

					object uniformValue = _selectedMaterial.Shader.Uniforms[shaderUniforms[i].Name];
					System.Numerics.Vector4 col = ((Vector4) uniformValue).ToNumerics();

					if (ImGui.ColorEdit4("", ref col))
					{
						//selectedMaterial
						int lastShader = ShaderCache.ShaderInUse;
						ShaderCache.UseShader(_selectedMaterial.Shader);

						_selectedMaterial.Shader.SetColor(shaderUniforms[i].Name, col);
						ShaderCache.UseShader(lastShader);
					}
				}

				if (shaderUniforms[i].Type == typeof(float))
				{
					ImGui.SameLine(ImGui.GetWindowWidth() - 200 - 5);
					ImGui.SetNextItemWidth(itemWidth);

					if (_selectedMaterial.Shader.Uniforms.ContainsKey(shaderUniforms[i].Name) == false)
					{
						_selectedMaterial.Shader.Uniforms[shaderUniforms[i].Name] = Activator.CreateInstance(shaderUniforms[i].Type);
					}

					object uniformValue = _selectedMaterial.Shader.Uniforms[shaderUniforms[i].Name];
					float fl = (float) uniformValue;

					if (ImGui.InputFloat("xxx", ref fl))
					{
						//selectedMaterial
						int lastShader = ShaderCache.ShaderInUse;
						ShaderCache.UseShader(_selectedMaterial.Shader);

						_selectedMaterial.Shader.SetFloat(shaderUniforms[i].Name, fl);
						ShaderCache.UseShader(lastShader);
					}
				}
			}
		}
	}
	*/
}