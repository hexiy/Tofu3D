using ImGuiNET;
using Tofu3D.Physics;

namespace Tofu3D;

public class EditorPanelSceneView : EditorPanel
{
	public static EditorPanelSceneView I { get; private set; }

	public override void Draw()
	{
		if (Active == false)
		{
			return;
		}

		ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
		if (Global.EditorAttached)
		{
			Editor.SceneViewSize = Camera.I.Size + new Vector2(0, 50);

			ImGui.SetNextWindowSize(Camera.I.Size + new Vector2(0, 50), ImGuiCond.Always);
			ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always, new Vector2(0, 0));
			ImGui.Begin("Scene View",
			            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

			ImGui.SetCursorPosX(Camera.I.Size.X / 2 - 150);

			Vector4 activeColor = ImGui.GetStyle().Colors[(int) ImGuiCol.Text];
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

			ImGui.SetCursorPosX(0);
			Editor.SceneViewPosition = new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY());
			ImGui.Image((IntPtr) Window.I.SceneRenderTexture.ColorAttachment, Camera.I.Size,
			            new Vector2(0, 1), new Vector2(1, 0));

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
			ImGui.Image((IntPtr) Window.I.SceneRenderTexture.ColorAttachment, Camera.I.Size,
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