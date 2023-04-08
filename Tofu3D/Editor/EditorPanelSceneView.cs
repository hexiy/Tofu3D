using ImGuiNET;
using Tofu3D.Physics;
using Tofu3D.Rendering;

namespace Tofu3D;

public class EditorPanelSceneView : EditorPanel
{
	public override string Name => "Scene View";
	public static EditorPanelSceneView I { get; private set; }
	static bool _renderCameraViews = true;

	public override void Draw()
	{
		if (Active == false)
		{
			return;
		}


		if (Global.EditorAttached)
		{
			ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
			ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

			Editor.SceneViewSize = Camera.I.Size + new Vector2(0, 50);

			ImGui.SetNextWindowSize(Camera.I.Size + new Vector2(0, 50), ImGuiCond.FirstUseEver);
			ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.FirstUseEver, new Vector2(0, 0));
			ImGui.Begin(Name, Editor.ImGuiDefaultWindowFlags | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);


			if ((Vector2) ImGui.GetWindowSize() != Camera.I.Size)
			{
				Camera.I.SetSize(ImGui.GetWindowSize());
			}

			ImGui.SetCursorPosX(Camera.I.Size.X / 2 - 150);

			Vector4 activeColor = Color.ForestGreen.ToVector4(); //ImGui.GetStyle().Colors[(int) ImGuiCol.Text];
			Vector4 inactiveColor = ImGui.GetStyle().Colors[(int) ImGuiCol.TextDisabled];
			ImGui.PushStyleColor(ImGuiCol.Text, PhysicsController.Running ? activeColor : inactiveColor);
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

			ImGui.SameLine();

			ImGui.PushStyleColor(ImGuiCol.Text, Global.GameRunning ? activeColor : inactiveColor);

			bool playButtonClicked = ImGui.Button("play");
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

			ImGui.PopStyleColor();

			ImGui.SameLine();
			bool resetDataButtonClicked = ImGui.Button("delete data");
			if (resetDataButtonClicked)
			{
				PersistentData.DeleteAll();
			}

			ImGui.SameLine();
			_renderCameraViews = GameObjectSelectionManager.GetSelectedGameObject()?.GetComponent<DirectionalLight>() != null;

			// ImGui.SetCursorPosX(0);
			ImGui.SetCursorPos(new Vector2(0, 70));
			Editor.SceneViewPosition = new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY());

			ImGui.Image((IntPtr) RenderPassSystem.FinalRenderTexture.ColorAttachment, RenderPassSystem.FinalRenderTexture.Size,
			            new Vector2(0, 1), new Vector2(1, 0), Color.White.ToVector4(), Color.Aqua.ToVector4());

			// ImGui.Image((IntPtr) RenderPassManager.FinalRenderTexture.ColorAttachment, RenderPassManager.FinalRenderTexture.Size * 0.9f,
			//             new Vector2(-0.5f, 0.5f), new Vector2(0.5f, -0.5f), Color.White.ToVector4(), Color.Aqua.ToVector4());
			// if (RenderPassOpaques.I != null)
			// {
			// 	ImGui.Image((IntPtr) RenderPassOpaques.I.PassRenderTexture.ColorAttachment, RenderPassOpaques.I.PassRenderTexture.Size,
			// 	           new Vector2(0, 1), new Vector2(1, 0));
			// }

			if (_renderCameraViews && RenderPassDirectionalLightShadowDepth.I != null)
			{
				float ratio = RenderPassDirectionalLightShadowDepth.I.DepthMapRenderTexture.Size.Y / RenderPassDirectionalLightShadowDepth.I.DepthMapRenderTexture.Size.X;
				float sizeX = Mathf.ClampMax(RenderPassDirectionalLightShadowDepth.I.DepthMapRenderTexture.Size.X, 400);
				float sizeY = sizeX * ratio;

				ImGui.SetCursorPos(new Vector2(0, 75));

				ImGui.Image((IntPtr) RenderPassDirectionalLightShadowDepth.I.DepthMapRenderTexture.ColorAttachment, new Vector2(sizeX, sizeY),
				            new Vector2(0, 1), new Vector2(1, 0), Color.White.ToVector4(), Color.Red.ToVector4());
			}

			ImGui.End();

			ImGui.PopStyleVar();
			ImGui.PopStyleVar();
		}
		else
		{
			ImGui.SetNextWindowSize(Camera.I.Size + new Vector2(0, 50), ImGuiCond.Always);
			ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always, new Vector2(0, 0));
			ImGui.Begin("Scene View",
			            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDecoration);

			ImGui.SetCursorPosX(0);
			Editor.SceneViewPosition = new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY());
			ImGui.Image((IntPtr) RenderPassSystem.FinalRenderTexture.ColorAttachment, Camera.I.Size,
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