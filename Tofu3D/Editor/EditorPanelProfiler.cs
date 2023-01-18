using System.Linq;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelProfiler : EditorPanel
{
	List<float> _physicsThreadSamples = new();
	List<float> _sceneRenderSamples = new();
	List<float> _imGuiSamples = new();
	List<float> _sceneUpdateSamples = new();
	public static EditorPanelProfiler I { get; private set; }

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

		ImGui.SetNextWindowSize(new Vector2(800, Window.I.ClientSize.Y - Editor.SceneViewSize.Y + 1), ImGuiCond.Always);
		ImGui.SetNextWindowPos(new Vector2(Window.I.ClientSize.X, Window.I.ClientSize.Y), ImGuiCond.Always, new Vector2(1, 1));
		//ImGui.SetNextWindowBgAlpha (0);
		ImGui.Begin("Profiler", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

		ImGui.Text($"GameObjects in scene: {Scene.I.GameObjects.Count}");

		for (int i = 0; i < Debug.Stats.Count; i++)
		{
			ImGui.Text($"{Debug.Stats.Keys.ElementAt(i)} : {Debug.Stats.Values.ElementAt(i)}");
		}

		for (int i = 0; i < Debug.Timers.Count; i++)
		{
			float msDuration = (float) Math.Round(Debug.Timers[Debug.Timers.Keys.ElementAt(i)].Elapsed.TotalMilliseconds, 2);
			if (Debug.Timers.Keys.ElementAt(i) == "Scene Update")
			{
				_sceneUpdateSamples.Add(msDuration);

				if (_sceneUpdateSamples.Count > 200)
				{
					_sceneUpdateSamples.RemoveAt(0);
				}

				ImGui.PlotLines("", ref _sceneUpdateSamples.ToArray()[0], _sceneUpdateSamples.Count, 0, $"Scene Update Time:{_sceneUpdateSamples.Last()} ms            ", 0, _sceneUpdateSamples.Max() + 1,
				                new Vector2(ImGui.GetContentRegionAvail().X, 100));
			}
			else if (Debug.Timers.Keys.ElementAt(i) == "Scene Render")
			{
				_sceneRenderSamples.Add(msDuration);

				if (_sceneRenderSamples.Count > 200)
				{
					_sceneRenderSamples.RemoveAt(0);
				}

				ImGui.PlotLines("", ref _sceneRenderSamples.ToArray()[0], _sceneRenderSamples.Count, 0, $"Scene Render Time:{_sceneRenderSamples.Last()} ms            ", 0, _sceneRenderSamples.Max() + 1,
				                new Vector2(ImGui.GetContentRegionAvail().X, 100));
			}
			else if (Debug.Timers.Keys.ElementAt(i) == "ImGui")
			{
				_imGuiSamples.Add(msDuration);

				if (_imGuiSamples.Count > 200)
				{
					_imGuiSamples.RemoveAt(0);
				}

				ImGui.PlotLines("", ref _imGuiSamples.ToArray()[0], _imGuiSamples.Count, 0, $"ImGui:{_imGuiSamples.Last()} ms            ", 0, _imGuiSamples.Max() + 1,
				                new Vector2(ImGui.GetContentRegionAvail().X, 100));
			}
			else if (Debug.Timers.Keys.ElementAt(i) == "Physics thread")
			{
				_physicsThreadSamples.Add(msDuration);

				if (_physicsThreadSamples.Count > 200)
				{
					_physicsThreadSamples.RemoveAt(0);
				}

				ImGui.PlotLines("", ref _physicsThreadSamples.ToArray()[0], _physicsThreadSamples.Count, 0, $"Physics Update Time:{_physicsThreadSamples.Last()} ms            ", 0, _physicsThreadSamples.Max() + 1,
				                new Vector2(ImGui.GetContentRegionAvail().X, 100));
			}
			else
			{
				float timerDuration = (float) Debug.Timers.Values.ElementAt(i).Elapsed.TotalMilliseconds;
				ImGui.PushStyleColor(ImGuiCol.Text, Color.Lerp(Color.White, Color.Red, Mathf.Clamp(timerDuration / 40 - 1, 0, 1)).ToVector4());
				ImGui.Text($"{Debug.Timers.Keys.ElementAt(i)} : {timerDuration} ms");
				ImGui.PopStyleColor();
			}
		}
		//ResetID();

		ImGui.End();
	}

	public override void Update()
	{
	}
}