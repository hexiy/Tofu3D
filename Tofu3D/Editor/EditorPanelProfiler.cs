﻿using System.Diagnostics;
using System.Linq;
using ImGuiNET;

namespace Tofu3D;

public class EditorPanelProfiler : EditorPanel
{
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

		ImGui.SetNextWindowSize(new Vector2(800, Window.I.ClientSize.Y - Editor.SceneViewSize.Y + 1), ImGuiCond.FirstUseEver);
		ImGui.SetNextWindowPos(new Vector2(Window.I.ClientSize.X, Window.I.ClientSize.Y), ImGuiCond.FirstUseEver, new Vector2(1, 1));
		//ImGui.SetNextWindowBgAlpha (0);
		ImGui.Begin("Profiler", Editor.ImGuiDefaultWindowFlags);

		ImGui.Text($"GameObjects in scene: {Scene.I.GameObjects.Count}");

		foreach (KeyValuePair<string, float> stat in Debug.Stats)
		{
			ImGui.Text($"{stat.Key} : {stat.Value}");
		}

		foreach (KeyValuePair<string, float> stat in Debug.AdditiveStats)
		{
			ImGui.Text($"{stat.Key} : {stat.Value}");
		}

		DebugGraphTimer.SourceGroup currentSourceGroup = DebugGraphTimer.SourceGroup.None;

		foreach (KeyValuePair<string, DebugGraphTimer> timerPair in Debug.GraphTimers)
		{
			float msDuration = (float) Math.Round(timerPair.Value.Stopwatch.Elapsed.TotalMilliseconds, 2);
			// float msDuration = (float) timerPair.Value.Stopwatch.Elapsed.TotalMilliseconds;
			float msDurationSlower = timerPair.Value.Sample10FramesAgo;
			timerPair.Value.AddSample(msDuration);

			if (timerPair.Value.Group != currentSourceGroup)
			{
				ImGui.SetWindowFontScale(1.3f);
				ImGui.TextColored(Color.Green.ToVector4(), timerPair.Value.Group.ToString().ToUpper());
				currentSourceGroup = timerPair.Value.Group;
				ImGui.SetWindowFontScale(1);
			}

			bool redlineHasValue = timerPair.Value.Redline.HasValue;
			if (redlineHasValue)
			{
				ImGui.PushStyleColor(ImGuiCol.Text, Color.Lerp(Color.Black, Color.Red, Mathf.Clamp(msDuration / timerPair.Value.Redline.Value, 0, 1)).ToVector4());
			}


			if (redlineHasValue)
			{
				ImGui.PushStyleVar(ImGuiStyleVar.DisabledAlpha, 1);
				// dont change alpha, we only BeginDisable so we dont see any hover toolips
			}

			bool disableHover = ImGui.IsMouseClicked(ImGuiMouseButton.Left) == false;
			if (disableHover)
			{
				ImGui.BeginDisabled();
			}

			bool clickedOnAnyControl = false;
			ImGui.PushStyleColor(ImGuiCol.PlotHistogram, Color.DarkRed.ToVector4());
			int plotWidth = (int) ImGui.GetContentRegionAvail().X;
			if (timerPair.Value.Samples.Length != plotWidth)
			{
				timerPair.Value.SetSamplesBufferSize(plotWidth);
			}

			if (timerPair.Value.Collapsed)
			{
				//ImGui.Indent();

				// ImGui.SetCursorPosX(timerPair.Value.LabelWidth);
				ImGui.PlotHistogram(string.Empty, ref timerPair.Value.Samples[0], timerPair.Value.Samples.Length, timerPair.Value.Offset, $"",
				                    timerPair.Value.MinSample, timerPair.Value.MaxSample,
				                    new Vector2(ImGui.GetContentRegionAvail().X, 30));
				ImGui.SameLine();

				clickedOnAnyControl |= ImGui.IsItemClicked();
				ImGui.SetCursorPosX(0);
				ImGui.Indent();

				ImGui.Text($"{timerPair.Value.Label}:{msDurationSlower.ToString("F2")} ms");

				ImGui.Unindent();
			}
			else
			{
				// ImGui.PlotHistogram(string.Empty, ref timerPair.Value.Samples[0], timerPair.Value.Samples.Length, timerPair.Value.Offset,
				//                     $"{timerPair.Value.Label}:{msDurationSlower.ToString("F2")} ms",
				//                     timerPair.Value.MinSample, timerPair.Value.MaxSample,
				//                     new Vector2(plotWidth, 100));

				ImGui.PlotHistogram(string.Empty, ref timerPair.Value.Samples[0], timerPair.Value.Samples.Length, timerPair.Value.Offset,
				                    $"{timerPair.Value.Label}:{msDurationSlower.ToString("F2")} ms",
				                    timerPair.Value.MinSample, timerPair.Value.MaxSample,
				                    new Vector2(plotWidth, 100));
			}

			ImGui.PopStyleColor();

			clickedOnAnyControl |= ImGui.IsItemClicked();

			if (disableHover)
			{
				ImGui.EndDisabled();
			}

			if (redlineHasValue)
			{
				ImGui.PopStyleVar();
			}

			if (clickedOnAnyControl)
			{
				timerPair.Value.Collapsed = !timerPair.Value.Collapsed;
			}

			ImGui.Separator();

			if (redlineHasValue)
			{
				ImGui.PopStyleColor();
			}

			// float timerDuration = (float) timerPair.Value.Stopwatch.Elapsed.TotalMilliseconds;
			// ImGui.PushStyleColor(ImGuiCol.Text, Color.Lerp(Color.White, Color.Red, Mathf.Clamp(timerDuration / 40 - 1, 0, 1)).ToVector4());
			// ImGui.Text($"{timerPair.Key} : {timerDuration} ms");
			// ImGui.PopStyleColor();
			//ResetID();
		}

		ImGui.End();
	}

	public override void Update()
	{
	}
}