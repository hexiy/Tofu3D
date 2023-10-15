﻿using ImGuiNET;
using Tofu3D.Physics;
using Tofu3D.Rendering;

namespace Tofu3D;

public class EditorPanelSceneView : EditorPanel
{
	public override string Name => "Scene View";
	public static EditorPanelSceneView I { get; private set; }
	static bool _renderCameraViews = true;

	bool _renderModeWindowOpened = false;
	bool _renderPassesWindowOpened = false;

	public override void Draw()
	{
		if (Active == false)
		{
			return;
		}

		if (Global.EditorAttached)
		{
			int tooltipsPanelHeight = 70;
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

			Tofu.Editor.SceneViewSize = Tofu.RenderPassSystem.FinalRenderTexture.Size + new Vector2(0, tooltipsPanelHeight);

			ImGui.SetNextWindowSize(Tofu.RenderPassSystem.FinalRenderTexture.Size, ImGuiCond.FirstUseEver);
			ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.FirstUseEver, new Vector2(0, 0));

			ImGui.Begin(Name, Editor.ImGuiDefaultWindowFlags | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

			if ((Vector2) ImGui.GetWindowSize() - new Vector2(0, tooltipsPanelHeight) != Camera.MainCamera.Size)
			{
				Camera.MainCamera.SetSize(ImGui.GetWindowSize() - new System.Numerics.Vector2(0, tooltipsPanelHeight));
				Debug.Log("SetSize");
			}

			ImGui.SetCursorPosX(Camera.MainCamera.Size.X / 2 - 400);

			Vector4 activeColor = Color.ForestGreen.ToVector4(); //ImGui.GetStyle().Colors[(int) ImGuiCol.Text];
			Vector4 inactiveColor = ImGui.GetStyle().Colors[(int) ImGuiCol.TextDisabled];
			/*ImGui.PushStyleColor(ImGuiCol.Text, PhysicsController.Running ? activeColor : inactiveColor);
			bool physicsButtonClicked = ImGui.Button("physics");
			if (physicsButtonClicked)
			{
				if (PhysicsController.Running == false)
				{
					PhysicsController.StartPhysics();
				}
				else if (PhysicsController.Running)
				{
					PhysicsController.StopPhysics();
				}
			}

			ImGui.PopStyleColor();

			ImGui.SameLine();*/
//////////
			bool renderPassesButtonClicked = ImGui.Button("Render passes");

			if (renderPassesButtonClicked)
			{
				_renderPassesWindowOpened = !_renderPassesWindowOpened;
				if (_renderPassesWindowOpened)
				{
					ImGui.OpenPopup("Render passes");
				}
			}

			if (_renderPassesWindowOpened)
			{
				if (ImGui.BeginPopupContextWindow("Render passes"))
				{
					foreach (RenderPass renderPass in Tofu.RenderPassSystem.RenderPasses)
					{
						bool isEnabled = renderPass.Enabled;
						bool wasEnabled = isEnabled;
						bool clicked = ImGui.Checkbox(renderPass.RenderPassType.ToString(), ref isEnabled);

						if (clicked)
						{
							renderPass.Enabled = !renderPass.Enabled;
						}
					}

					ImGui.EndPopup();
				}

				if (ImGui.IsPopupOpen("Render passes") == false && _renderPassesWindowOpened)
				{
					// clicked away
					_renderPassesWindowOpened = false;
				}
			}

			ImGui.SameLine();
			//////////
			bool renderModeButtonClicked = ImGui.Button("Render mode");

			if (renderModeButtonClicked)
			{
				_renderModeWindowOpened = !_renderModeWindowOpened;
				if (_renderModeWindowOpened)
				{
					ImGui.OpenPopup("RenderMode");
				}
			}

			if (_renderModeWindowOpened)
			{
				if (ImGui.BeginPopupContextWindow("RenderMode"))
				{
					foreach (ViewRenderMode mode in Enum.GetValues(typeof(ViewRenderMode)))
					{
						bool isEnabled = Tofu.RenderSettings.CurrentRenderModeSettings.CurrentRenderMode == mode;
						bool wasEnabled = isEnabled;
						bool clicked = ImGui.Checkbox(mode.ToString(), ref isEnabled);
						bool hovered = ImGui.IsItemHovered();
						if (hovered)
						{
							bool isNew = Tofu.RenderSettings.CurrentRenderModeSettings.CurrentRenderMode != mode;
							Tofu.RenderSettings.CurrentRenderModeSettings.CurrentRenderMode = mode;
							if (isNew)
							{
								Tofu.RenderSettings.SaveData();
							}
						}

						if (clicked)
						{
							_renderModeWindowOpened = false;
						}
					}

					ImGui.EndPopup();
				}

				if (ImGui.IsPopupOpen("RenderMode") == false && _renderModeWindowOpened)
				{
					// clicked away
					_renderModeWindowOpened = false;
				}
			}

			ImGui.SameLine();

			//////////
			/// 
			ImGui.PushStyleColor(ImGuiCol.Text, Tofu.RenderSettings.CurrentWireframeRenderSettings.WireframeVisible ? activeColor : inactiveColor);
			bool wireframeButtonClicked = ImGui.Button("Wireframe");
			if (wireframeButtonClicked)
			{
				Tofu.RenderSettings.CurrentWireframeRenderSettings.WireframeVisible = !Tofu.RenderSettings.CurrentWireframeRenderSettings.WireframeVisible;
				Tofu.RenderSettings.SaveData();
			}

			ImGui.PopStyleColor();

			ImGui.SameLine();

			ImGui.PushStyleColor(ImGuiCol.Text, Global.GameRunning ? activeColor : inactiveColor);

			bool playButtonClicked = ImGui.Button("play");

			ImGui.PopStyleColor();

			if (playButtonClicked)
			{
				if (Global.GameRunning)
				{
					Playmode.PlayMode_Stop();
				}
				else
				{
					Playmode.PlayMode_Start();
				}
			}

			ImGui.SameLine();

			ImGui.SetNextItemWidth(200);

			string projectionModeButtonText = Tofu.SceneViewController.CurrentProjectionMode == ProjectionMode.Orthographic ? "2D" : "3D";
			bool projectionButtonClicked = ImGui.Button(projectionModeButtonText);
			if (projectionButtonClicked)
			{
				if (Tofu.SceneViewController.CurrentProjectionMode == ProjectionMode.Orthographic)
				{
					Tofu.SceneViewController.SetProjectionMode(ProjectionMode.Perspective);
				}
				else
				{
					Tofu.SceneViewController.SetProjectionMode(ProjectionMode.Orthographic);
				}
			}

			_renderCameraViews = Global.Debug && GameObjectSelectionManager.GetSelectedGameObject()?.GetComponent<DirectionalLight>() != null;
			// _renderCameraViews = true;
			// ImGui.SetCursorPosX(0);
			ImGui.SetCursorPos(new Vector2(0, tooltipsPanelHeight));

			Tofu.Editor.SceneViewPosition = new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY());

			if (Tofu.RenderPassSystem.CanRender)
			{
				ImGui.Image((IntPtr) Tofu.RenderPassSystem.FinalRenderTexture.ColorAttachment, Tofu.RenderPassSystem.FinalRenderTexture.Size,
				            new Vector2(0, 1), new Vector2(1, 0));
			}
			else
			{
				ImGui.Dummy(Tofu.RenderPassSystem.FinalRenderTexture.Size);
			}

			// ImGui.Image((IntPtr) RenderPassManager.FinalRenderTexture.ColorAttachment, RenderPassManager.FinalRenderTexture.Size * 0.9f,
			//             new Vector2(-0.5f, 0.5f), new Vector2(0.5f, -0.5f), Color.White.ToVector4(), Color.Aqua.ToVector4());
			// if (RenderPassOpaques.I != null)
			// {
			// 	ImGui.Image((IntPtr) RenderPassOpaques.I.PassRenderTexture.ColorAttachment, RenderPassOpaques.I.PassRenderTexture.Size,
			// 	           new Vector2(0, 1), new Vector2(1, 0));
			// }
			if (_renderCameraViews && RenderPassDirectionalLightShadowDepth.I?.DebugGrayscaleTexture != null)
			{
				float ratio = RenderPassDirectionalLightShadowDepth.I.DebugGrayscaleTexture.Size.Y / RenderPassDirectionalLightShadowDepth.I.DebugGrayscaleTexture.Size.X;
				float sizeX = Mathf.ClampMax(RenderPassDirectionalLightShadowDepth.I.DebugGrayscaleTexture.Size.X, 400);
				float sizeY = sizeX * ratio;

				ImGui.SetCursorPos(new Vector2(5, 75));

				ImGui.Image((IntPtr) RenderPassDirectionalLightShadowDepth.I.DebugGrayscaleTexture.ColorAttachment, new Vector2(sizeX, sizeY),
				            new Vector2(0, 1), new Vector2(1, 0), Color.White.ToVector4(), Color.Red.ToVector4());
			}

			ImGui.End();

			ImGui.PopStyleVar();

			ImGui.PopStyleVar();
		}

		else

		{
			ImGui.SetNextWindowSize(Camera.MainCamera.Size + new Vector2(0, 50), ImGuiCond.Always);
			ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always, new Vector2(0, 0));
			ImGui.Begin("Scene View",
			            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDecoration);

			ImGui.SetCursorPosX(0);
			Tofu.Editor.SceneViewPosition = new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY());
			ImGui.Image((IntPtr) Tofu.RenderPassSystem.FinalRenderTexture.ColorAttachment, Camera.MainCamera.Size,
			            new Vector2(0, 1), new Vector2(1, 0));

			ImGui.End();
		}
	}

	public override void Update()
	{
	}

	public override void Init()
	{
		I = this;
	}
}