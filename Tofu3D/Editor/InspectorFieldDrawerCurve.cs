using ImGuiNET;

namespace Tofu3D;

public class InspectorFieldDrawerCurve : InspectorFieldDrawable<Curve>
{
    private int draggingPointIndex = -1;

    public override void Draw(FieldOrPropertyInfo info, InspectableData componentInspectorData)
    {
        Vector2 graphSize = new(300, 100);

        Vector2 pos = ImGui.GetCursorPos();
        Vector2 screenPos = TofuImGui.GetCursorScreenPos();
        // screenPos = screenPos * new Vector2(1, -1);
        Vector2 mousePos = Tofu.MouseInput.PositionInWindow;
        Vector2 mousePosRelativeToGraph =
            (mousePos - screenPos) * Tofu.Window.MonitorScale + new Vector2(0, graphSize.Y);
        Vector2 mousePosInGraphNormalizedCoordinates = mousePosRelativeToGraph / graphSize;

        Curve curve = GetValue(info, componentInspectorData);

        ImGui.PlotLines(string.Empty, ref curve._points[0], curve._points.Length, 0,
            string.Empty,
            0, 1,
            graphSize);

        ImGui.PushClipRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), false);
        bool cursorIsInsideGraph = ImGui.IsItemHovered();
        bool doubleClickedGraph = cursorIsInsideGraph && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);

        Vector2 cursorPos = ImGui.GetCursorPos();

        for (int i = 0; i < curve.DefiningPoints.Count; i++)
        {
            ImGui.SameLine();

            Vector2 newPos = new(pos.X + curve.DefiningPoints[i].X * graphSize.X,
                pos.Y + (1 - curve.DefiningPoints[i].Y) * graphSize.Y);


            Texture texture = Tofu.AssetManager.Load<Texture>("Resources/dot.png");

            Color circleColor = Color.Purple;

            ImGui.SetCursorPos(newPos - new Vector2(15));
            ImGui.Image(texture.TextureId, new Vector2(30), new Vector2(0), new Vector2(1), circleColor.ToVector4());

            bool cursorHoversCurrentPoint = ImGui.IsItemHovered();
            bool currentPointIsClicked = cursorHoversCurrentPoint && Tofu.MouseInput.IsButtonDown(MouseButtons.Button1);
            if (cursorHoversCurrentPoint) circleColor = Color.MidnightBlue;

            if (currentPointIsClicked) circleColor = Color.IndianRed;


            ImGui.SetCursorPos(newPos - new Vector2(15));

            ImGui.Image(texture.TextureId, new Vector2(30), new Vector2(0), new Vector2(1), circleColor.ToVector4());

            if (draggingPointIndex != -1 && Tofu.MouseInput.IsButtonDown(MouseButtons.Button1) == false)
            {
                draggingPointIndex = -1;
            }
            else if (draggingPointIndex == -1)
            {
                bool x = cursorHoversCurrentPoint && Tofu.MouseInput.IsButtonDown(MouseButtons.Button1);
                x = x || currentPointIsClicked;
                if (x) draggingPointIndex = i;
            }

            if (draggingPointIndex == i && cursorIsInsideGraph)
            {
                // curve.DefiningPoints[i] += Tofu.MouseInput.ScreenDelta / graphSize * 2;
                // 
                curve.DefiningPoints[i] = mousePosInGraphNormalizedCoordinates;

                // Vector2 newPos = new Vector2(pos.X + (curve.DefiningPoints[i].X * graphSize.X),
                //     pos.Y + (1 - curve.DefiningPoints[i].Y) * graphSize.Y);
                curve.RecalculateCurve();
                // Debug.Log($"Dragging curve point:{curve.DefiningPoints[i]}");
            }

            if (Tofu.MouseInput.ButtonReleased())
            {
                // curve.RecalculateCurve();
            }

            // bool doubleClicked = ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);
            bool removePoint = ImGui.IsMouseClicked(ImGuiMouseButton.Left) && KeyboardInput.IsKeyDown(Keys.LeftControl);
            if (removePoint && cursorHoversCurrentPoint && curve.CanRemovePoint)
            {
                // remoove the hovered point
                curve.DefiningPoints.RemoveAt(i);
                Debug.Log("Removed point");
                doubleClickedGraph = false;
                curve.RecalculateCurve();
                break;
            }
        }

        if (doubleClickedGraph)
        {
            curve.AddDefiningPoint(mousePosInGraphNormalizedCoordinates);
            curve.RecalculateCurve();
        }

        ImGui.SetCursorPos(cursorPos); // so the next imgui element doesnt move with control points
        ImGui.PopClipRect();
    }
}