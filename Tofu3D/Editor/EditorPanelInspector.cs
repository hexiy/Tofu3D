using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelInspector : EditorPanel
{
	string _addComponentPopupText = "";
	int _contentMaxWidth;

	GameObject _selectedGameObject;
	Material _selectedMaterial;
	public static EditorPanelInspector I { get; private set; }

	List<ComponentInspectorData> _currentComponents = new List<ComponentInspectorData>(); // whenever we select new gameobject, cache component data

	public override void Init()
	{
		I = this;

		_componentTypes = typeof(Component).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract).ToList();
		Scene.AnyComponentAddedToScene += OnComponentAddedToScene;
	}

	void OnComponentAddedToScene(Component comp)
	{
		if (_selectedGameObject == comp.GameObject)
		{
			UpdateCurrentComponentsCache();
			return;
		}
	}

	public override void Update()
	{
	}

	public void OnGameObjectsSelected(List<int> ids)
	{
		if (ids.Count != 1)
		{
			_selectedGameObject = null;
		}
		else
		{
			_selectedGameObject = Scene.I.GetGameObject(ids[0]);
			UpdateCurrentComponentsCache();
			_selectedMaterial = null;
		}
	}

	private void UpdateCurrentComponentsCache()
	{
		if (_selectedGameObject == null)
		{
			return;
		}

		_currentComponents.Clear();
		for (int componentIndex = 0; componentIndex < _selectedGameObject.Components.Count; componentIndex++)
		{
			ComponentInspectorData data = new ComponentInspectorData(_selectedGameObject.Components[componentIndex]);
			_currentComponents.Add(data);
		}
	}

	public void OnMaterialSelected(string materialPath)
	{
		_selectedMaterial = MaterialCache.GetMaterial(Path.GetFileName(materialPath)); //MaterialAssetManager.LoadMaterial(materialPath);

		OnGameObjectsSelected(new List<int>());
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
		ImGui.SetNextWindowPos(new Vector2(Window.I.ClientSize.X, 0), ImGuiCond.FirstUseEver, new Vector2(1, 0));
		//ImGui.SetNextWindowBgAlpha (0);
		ImGui.Begin("Inspector", Editor.ImGuiDefaultWindowFlags | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar);

		ImGui.Text("Inspector");

		ImGui.SameLine();

		//bool debugButtonClicked = ImGui.Button("Debug");
		ImGui.SetCursorPosX(715);
		bool debugButtonClicked = ImGui.SmallButton("Debug");
		if (debugButtonClicked)
		{
			Global.Debug = !Global.Debug;
		}

		ImGui.Spacing();
		ResetId();


		if (_selectedGameObject != null)
		{
			ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2);
			DrawGameObjectInspector();
			ImGui.PopStyleVar(1);
		}

		if (_selectedMaterial != null)
		{
			DrawMaterialInspector();
		}

		ImGui.End();
	}

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
		                                                   typeof(AudioClip)
	                                                   };
	List<Type> _componentTypes;

	void DrawGameObjectInspector()
	{
		if (_selectedGameObject.IsPrefab)
		{
			if (ImGui.Button("Update prefab"))
			{
				Serializer.I.SaveGameObject(_selectedGameObject, _selectedGameObject.PrefabPath);
			}

			if (ImGui.Button("Delete prefab"))
			{
				_selectedGameObject.IsPrefab = false;
			}
		}

		PushNextId();
		ImGui.SetScrollX(0);

		string gameObjectName = _selectedGameObject.Name;
		ImGui.Checkbox("", ref _selectedGameObject.ActiveSelf);
		ImGui.SameLine();
		PushNextId();
		ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
		if (ImGui.InputText("", ref gameObjectName, 100))
		{
			_selectedGameObject.Name = gameObjectName;
		}

		foreach (ComponentInspectorData componentInspectorData in _currentComponents)
		{
			PushNextId();

			//ImGui.SetNextItemWidth (300);
			ImGui.Checkbox("", ref componentInspectorData.Component.Enabled);
			ImGui.SameLine();

			if (ImGui.Button("-"))
			{
				_selectedGameObject.RemoveComponent(componentInspectorData.Component);
				continue;
			}

			ImGui.SameLine();
			PushNextId();

			if (ImGui.CollapsingHeader(componentInspectorData.ComponentType.Name, ImGuiTreeNodeFlags.DefaultOpen))
			{
				// if (componentInspectorData.Infos == null)
				// {
				// 	componentInspectorData.Infos = new FieldOrPropertyInfo[componentInspectorData.Fields.Length + componentInspectorData.Properties.Length];
				// }
				//
				// for (int fieldIndex = 0; fieldIndex < componentInspectorData.Fields.Length; fieldIndex++)
				// {
				// 	// // TODO FIX THIS MEMORY HOG
				// 	// if (componentInspectorData.Infos[fieldIndex] == null)
				// 	// {
				// 	// 	componentInspectorData.Infos[fieldIndex] = new FieldOrPropertyInfo(componentInspectorData.Fields[fieldIndex], componentInspectorData.Component);
				// 	// }
				// 	// else
				// 	// {
				// 		componentInspectorData.Infos[fieldIndex].SetInfo(componentInspectorData.Fields[fieldIndex], componentInspectorData.Component);
				// 	// }
				// }


				foreach (FieldOrPropertyInfo info in componentInspectorData.Infos)
				{
					if (info.CanShowInEditor == false)
					{
						continue;
					}

					PushNextId();

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
						System.Numerics.Vector3 systemv3 = (Vector3) info.GetValue(componentInspectorData.Component);
						if (ImGui.DragFloat3("", ref systemv3, 0.01f))
						{
							info.SetValue(componentInspectorData.Component, (Vector3) systemv3);
						}
					}
					else if (info.FieldOrPropertyType == typeof(Vector2))
					{
						System.Numerics.Vector2 systemv2 = (Vector2) info.GetValue(componentInspectorData.Component);
						if (ImGui.DragFloat2("", ref systemv2, 0.01f))
						{
							info.SetValue(componentInspectorData.Component, (Vector2) systemv2);
						}
					}
					else if (info.FieldOrPropertyType == typeof(AudioClip))
					{
						AudioClip audioClip = (AudioClip) info.GetValue(componentInspectorData.Component);
						if (audioClip == null)
						{
							audioClip = new AudioClip();
							info.SetValue(componentInspectorData.Component, audioClip);
						}

						string clipName = Path.GetFileName(audioClip?.Path);

						bool clicked = ImGui.Button(clipName, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));


						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("CONTENT_BROWSER_AUDIOCLIP", ImGuiDragDropFlags.None);
							string fileName = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && fileName.Length > 0)
							{
								// fileName = Path.GetRelativePath("Assets", fileName);

								audioClip.Path = fileName;
								info.SetValue(componentInspectorData.Component, audioClip);
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (info.FieldOrPropertyType == typeof(List<GameObject>))
					{
						List<GameObject> listOfGameObjects = (List<GameObject>) info.GetValue(componentInspectorData.Component);
						if (ImGui.Button("+"))
						{
							listOfGameObjects.Add(null);
							info.SetValue(componentInspectorData.Component, listOfGameObjects);
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
									info.SetValue(componentInspectorData.Component, listOfGameObjects);
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
										GameObject foundGo = Scene.I.GetGameObject(int.Parse(payload));
										listOfGameObjects[j] = foundGo;
										info.SetValue(componentInspectorData.Component, listOfGameObjects);
									}

									ImGui.EndDragDropTarget();
								}

								ImGui.PopStyleColor();
							}
						}
					}
					else if (info.FieldOrPropertyType == typeof(Action))
					{
						Action action = (Action) info.GetValue(componentInspectorData.Component);
						ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int) ImGuiCol.Text]);
						if (ImGui.Button($"> {info.Name} <", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight())))
						{
							action?.Invoke();
						}

						ImGui.PopStyleColor(1);
					}
					else if (info.FieldOrPropertyType == typeof(Texture) && componentInspectorData.Component is TextureRenderer)
					{
						string textureName = Path.GetFileName((componentInspectorData.Component as TextureRenderer).Texture?.Path);

						bool clicked = ImGui.Button(textureName, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
						//ImiGui.Text(textureName);
						if (clicked)
						{
							EditorPanelBrowser.I.GoToFile((componentInspectorData.Component as TextureRenderer).Texture.Path);
						}

						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("CONTENT_BROWSER_TEXTURE", ImGuiDragDropFlags.None);
							string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
							{
								payload = Path.GetRelativePath("Assets", payload);

								textureName = payload;

								(componentInspectorData.Component as TextureRenderer).LoadTexture(textureName);
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (info.FieldOrPropertyType == typeof(Material))
					{
						string materialPath = Path.GetFileName((componentInspectorData.Component as Renderer).Material.FilePath);

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
									(componentInspectorData.Component as Renderer).Material = draggedMaterial;
								}
								// load new material
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (info.FieldOrPropertyType == typeof(GameObject))
					{
						GameObject goObject = info.GetValue(componentInspectorData.Component) as GameObject;
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
									GameObject loadedGo = Serializer.I.LoadPrefab(payload, true);
									info.SetValue(componentInspectorData.Component, loadedGo);
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
									GameObject foundGo = Scene.I.GetGameObject(int.Parse(payload));
									info.SetValue(componentInspectorData.Component, foundGo);
								}
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (info.FieldOrPropertyType == typeof(Color))
					{
						System.Numerics.Vector4 fieldValue = ((Color) info.GetValue(componentInspectorData.Component)).ToVector4();

						if (ImGui.ColorEdit4("", ref fieldValue))
						{
							info.SetValue(componentInspectorData.Component, fieldValue.ToColor());
						}
					}
					else if (info.FieldOrPropertyType == typeof(bool))
					{
						ImGui.SameLine(ImGui.GetWindowWidth() - ImGui.GetContentRegionAvail().X / 2);

						bool fieldValue = (bool) info.GetValue(componentInspectorData.Component);

						if (ImGui.Checkbox("", ref fieldValue))
						{
							info.SetValue(componentInspectorData.Component, fieldValue);
						}
					}
					else if (info.FieldOrPropertyType == typeof(float))
					{
						float fieldValue = (float) info.GetValue(componentInspectorData.Component);

						SliderF sliderAttrib = null;
						List<CustomAttributeData> a = info.FieldOrPropertyType.CustomAttributes.ToList();
						for (int i = 0; i < info.CustomAttributes.Count(); i++)
						{
							if (info.CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(SliderF))
							{
								FieldInfo fieldType = componentInspectorData.Component.GetType().GetField(info.Name);
								if (fieldType != null)
								{
									sliderAttrib = fieldType.GetCustomAttribute<SliderF>();
								}
								else
								{
									PropertyInfo propertyType = componentInspectorData.Component.GetType().GetProperty(info.Name);
									sliderAttrib = propertyType.GetCustomAttribute<SliderF>();
								}
							}
						}

						if (sliderAttrib != null)
						{
							if (ImGui.SliderFloat("", ref fieldValue, sliderAttrib.MinValue, sliderAttrib.MaxValue))
							{
								info.SetValue(componentInspectorData.Component, fieldValue);
							}
						}
						else
						{
							if (ImGui.DragFloat("", ref fieldValue, 0.01f, float.NegativeInfinity, float.PositiveInfinity, "%.05f"))
							{
								info.SetValue(componentInspectorData.Component, fieldValue);
							}
						}
					}
					else if (info.FieldOrPropertyType == typeof(int))
					{
						int fieldValue = (int) info.GetValue(componentInspectorData.Component);


						if (ImGui.DragInt("", ref fieldValue))
						{
							info.SetValue(componentInspectorData.Component, fieldValue);
						}
					}
					else if (info.FieldOrPropertyType == typeof(string))
					{
						string fieldValue = info.GetValue(componentInspectorData.Component).ToString();

						if (ImGui.InputTextMultiline("", ref fieldValue, 100, new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, 200)))
						{
							info.SetValue(componentInspectorData.Component, fieldValue);
						}
					}

					if (info.IsReadonly)
					{
						ImGui.EndDisabled();
					}
					//ImGui.PopID();
				}

				//PropertyInfo[] properties = selectedGameObject.Components[i].GetType ().GetProperties ();
				//for (int j = 0; j < properties.Length; j++)
				//{
				//	PushNextID ();
				//	for (int k = 0; k < properties[j].CustomAttributes.Count (); k++)
				//	{
				//		if (properties[j].CustomAttributes.ElementAtOrDefault (k).AttributeType != typeof (ShowInEditor))
				//		{
				//			continue;
				//		}
				//		ImGui.Text (properties[j].Name);
				//		ImGui.SameLine ();
				//		//ImGui.Text (fieldInfo[j].GetValue (selectedGameObject.Components[i]).ToString ());

				//		//if (properties[j].PropertyType == typeof (float))
				//		//{
				//		//	float fl = (float) properties[j].GetValue (selectedGameObject.Components[i]);
				//		//	if (ImGui.DragFloat ("", ref fl, 0.01f, 0, 1))
				//		//	{
				//		//		properties[j].SetValue (selectedGameObject.Components[i], fl);
				//		//	}
				//		//}
				//		if (properties[j].PropertyType == typeof (Vector3))
				//		{
				//			Vector3 fl = (Vector3) properties[j].GetValue (selectedGameObject.Components[i]);
				//			if (ImGui.DragFloat3 ("", ref fl, 0.01f))
				//			{
				//				//properties[j].SetValue (selectedGameObject.Components[i], fl);
				//			}
				//		}
				//	}
				//	ImGui.PopID ();
				//}
			}
		}

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
							_selectedGameObject.AddComponent(_componentTypes[i]);
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
						_selectedGameObject.AddComponent(_componentTypes[i]);
						ImGui.CloseCurrentPopup();
					}
				}
			}

			ImGui.EndPopup();
		}
	}
}