using ImGuiNET;
using Tofu3D.Rendering;

namespace Tofu3D;

public class EditorPanelMenuBar : EditorPanel
{
	public static EditorPanelMenuBar I { get; private set; }
	public override string Name => "Menu Bar";

	public override void Init()
	{
		I = this;
	}

	public override void Draw()
	{
		if (Active == false)
		{
			return;
		}

		if (Global.EditorAttached)
		{
			ImGui.SetNextWindowSize(new Vector2(Tofu.I.Window.Size.X*Global.EditorScale, 50), ImGuiCond.FirstUseEver);
			ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.FirstUseEver, new Vector2(0, 0));
			// ImGui.PushStyleColor(ImGuiCol.WindowBg, Color.Red.ToVector4());
			// ImGui.Begin(Name, Editor.ImGuiDefaultWindowFlags | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar);
			ImGui.BeginMainMenuBar();
			
			bool layoutButtonClicked = ImGui.BeginMenu("Layout");
			// if (layoutButtonClicked)
			// {
			// 	ImGui.OpenPopup("Layout");
			// }

			if (layoutButtonClicked)
			{
				bool loadDefaultLayoutButtonClicked = ImGui.Button("Load Default Layout");
				if (loadDefaultLayoutButtonClicked)
				{
					ImGui.CloseCurrentPopup();

					Editor.BeforeDraw += EditorLayoutManager.LoadDefaultLayout; // load layout before drawing anything, otherwise we break the layout by calling imgui after this editor panel
				}

				bool saveDefaultLayoutButtonClicked = ImGui.Button("Save Default Layout");
				if (saveDefaultLayoutButtonClicked)
				{
					ImGui.CloseCurrentPopup();

					EditorLayoutManager.SaveDefaultLayout();
				}

				ImGui.EndMenu();
			}

			ImGui.EndMainMenuBar();

			// ImGui.PopStyleColor();
		}
	}

	public override void Update()
	{
	}
}