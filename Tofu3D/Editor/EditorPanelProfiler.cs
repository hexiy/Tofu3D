﻿using ImGuiNET;

namespace Tofu3D;

public class EditorPanelProfiler : EditorPanel
{
    public override string Name => "Profiler";
    public override Vector2 Size => new(800, Tofu.Window.ClientSize.Y - Tofu.Editor.SceneViewSize.Y + 1);
    public override Vector2 Position => new(Tofu.Window.ClientSize.X, Tofu.Window.ClientSize.Y);
    public override Vector2 Pivot => new(1, 1);
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

        SetWindow();

        ImGui.Text($"GameObjects in scene: {Tofu.SceneManager.CurrentScene.GameObjects.Count}");

        foreach (var stat in Debug.Stats)
        {
            ImGui.Text($"{stat.Value}");
        }

        foreach (var stat in Debug.AdditiveStats)
        {
            ImGui.Text($"{stat.Key} : {stat.Value}");
        }

        var currentSourceGroup = DebugGraphTimer.SourceGroup.None;

        foreach (var timerPair in Debug.GraphTimers)
        {
            var msDuration = (float)Math.Round(timerPair.Value.Stopwatch.Elapsed.TotalMilliseconds, 2);
            // float msDuration = (float) timerPair.Value.Stopwatch.Elapsed.TotalMilliseconds;
            var msDurationSlower = timerPair.Value.Sample10FramesAgo;
            timerPair.Value.AddSample(msDuration);

            if (timerPair.Value.Group != currentSourceGroup)
            {
                ImGui.SetWindowFontScale(1.3f);
                ImGui.TextColored(Color.Green.ToVector4(), timerPair.Value.Group.ToString().ToUpper());
                currentSourceGroup = timerPair.Value.Group;
                ImGui.SetWindowFontScale(1);
            }

            var redlineHasValue = timerPair.Value.Redline.HasValue;
            if (redlineHasValue)
            {
                ImGui.PushStyleColor(ImGuiCol.Text,
                    Color.Lerp(Color.Black, Color.Red, Mathf.Clamp(msDuration / timerPair.Value.Redline.Value, 0, 1))
                        .ToVector4());
            }


            if (redlineHasValue)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.DisabledAlpha, 1);
            }

            // dont change alpha, we only BeginDisable so we dont see any hover toolips
            var disableHover = ImGui.IsMouseClicked(ImGuiMouseButton.Left) == false;
            if (disableHover)
            {
                ImGui.BeginDisabled();
            }

            var clickedOnAnyControl = false;
            // ImGui.PushStyleColor(ImGuiCol.PlotHistogram, Color.DarkRed.ToVector4());


            if (timerPair.Value.Collapsed)
            {
                // ImGui.PlotHistogram(string.Empty, ref timerPair.Value.Samples[0], timerPair.Value.Samples.Length, timerPair.Value.Offset, $"",
                //                     timerPair.Value.MinSample, timerPair.Value.MaxSample,
                //                     new Vector2(ImGui.GetContentRegionAvail().X, 30));
                // ImGui.SameLine();

                clickedOnAnyControl |= ImGui.IsItemClicked();
                ImGui.SetCursorPosX(0);
                ImGui.Indent();

                ImGui.Text($"{timerPair.Value.Label}:{msDurationSlower.ToString("F3")} ms");

                ImGui.Unindent();
            }
            else
            {
                var plotWidth = (int)ImGui.GetContentRegionAvail().X;
                // if (timerPair.Value.Samples.Length != plotWidth && plotWidth > 0)
                // {
                // timerPair.Value.SetSamplesBufferSize((uint) plotWidth);
                // }

                ImGui.PlotHistogram(string.Empty, ref timerPair.Value.Samples[0], timerPair.Value.Samples.Length,
                    timerPair.Value.Offset,
                    $"{timerPair.Value.Label}:{msDurationSlower.ToString("F3")} ms",
                    timerPair.Value.MinSample, timerPair.Value.MaxSample,
                    new Vector2(plotWidth, 100));
            }

            // ImGui.PopStyleColor();

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