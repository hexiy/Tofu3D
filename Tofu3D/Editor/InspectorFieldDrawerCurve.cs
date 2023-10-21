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
            (mousePos - screenPos)*Tofu.Window.MonitorScale + new Vector2(0,(graphSize.Y));
        Debug.Log(mousePosRelativeToGraph);


        Curve curve = GetValue(info, componentInspectorData);

        ImGui.PlotLines(string.Empty, ref curve._points[0], curve._points.Length, 0,
            string.Empty,
            0, 1,
            graphSize);


        bool cursorIsInsideGraph = ImGui.IsItemHovered();
        bool doubleClickedGraph = ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);

        Vector2 cursorPos = ImGui.GetCursorPos();

        for (int i = 0; i < curve.DefiningPoints.Count; i++)
        {
            ImGui.SameLine();

            Vector2 newPos = new Vector2(pos.X + (curve.DefiningPoints[i].X * graphSize.X),
                pos.Y + (1 - curve.DefiningPoints[i].Y) * graphSize.Y);

            ImGui.SetCursorPos(newPos - new Vector2(15));

            Texture texture = Tofu.AssetManager.Load<Texture>("Resources/dot.png");

            ImGui.Image(texture.TextureId, new(30), new(0), new(1), Color.Purple.ToVector4());

            bool cursorHoversCurrentPoint = ImGui.IsItemHovered();
            if (draggingPointIndex != -1 && ImGui.IsMouseDown(ImGuiMouseButton.Left) == false)
            {
                draggingPointIndex = -1;
            }
            else if (draggingPointIndex == -1)
            {
                bool x = ImGui.IsItemHovered() && ImGui.IsMouseDragging(ImGuiMouseButton.Left);
                x = x || ImGui.IsItemClicked();
                if (x)
                {
                    draggingPointIndex = i;
                }
            }

            if (draggingPointIndex == i && cursorIsInsideGraph)
            {
                // curve.DefiningPoints[i] += Tofu.MouseInput.ScreenDelta / graphSize * 2;
                // 
                curve.DefiningPoints[i] = (mousePosRelativeToGraph / graphSize);

                // Vector2 newPos = new Vector2(pos.X + (curve.DefiningPoints[i].X * graphSize.X),
                //     pos.Y + (1 - curve.DefiningPoints[i].Y) * graphSize.Y);
                curve.RecalculateCurve();
                // Debug.Log($"Dragging curve point:{curve.DefiningPoints[i]}");
            }

            bool doubleClicked = ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);
            if (doubleClicked)
            {
                if (cursorHoversCurrentPoint)
                {
                    if (curve.CanRemovePoint)
                    {
                        // remoove the hovered point
                        curve.DefiningPoints.RemoveAt(i);
                        Debug.Log("Removed point");
                        doubleClickedGraph = false;
                        break;
                    }
                }
            }
        }

        if (doubleClickedGraph)
        {
            Vector2 cursorPosRelativeToCurveSpace =
                (Tofu.MouseInput.PositionInView - screenPos) / graphSize * 2 * new Vector2(1, 0);

            Debug.Log($"Added point to curve :{cursorPosRelativeToCurveSpace}");
            curve.AddDefiningPoint(cursorPosRelativeToCurveSpace);
        }

        ImGui.SetCursorPos(cursorPos); // so the next imgui element doesnt move with control points
    }
}