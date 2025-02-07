using ImGuiNET;

namespace Tofu3D;

public static class TofuImGui
{
    public static Vector2 DefaultItemSpacing { get; private set; }
    public static Vector2 DefaultWindowPadding { get; private set; }

    public static Vector2 GetCursorScreenPos() =>
        new System.Numerics.Vector2(ImGui.GetCursorScreenPos().X / Tofu.Window.MonitorScale,
            Tofu.Window.WindowSize.Y -
            ImGui.GetCursorScreenPos().Y / Tofu.Window.MonitorScale); // * new Vector2(-1, 1);

    public static void Init()
    {
        DefaultItemSpacing = ImGui.GetStyle().ItemSpacing;
        DefaultWindowPadding = ImGui.GetStyle().WindowPadding;
    }

    /// <summary>
    /// Returns true only on mouse released
    /// </summary>
    /// <param name="payloadTag"></param>
    /// <returns></returns>
    public static bool PayloadHasBeenDropped(string payloadTag)
    {
        unsafe
        {
            if (ImGui.AcceptDragDropPayload(payloadTag, ImGuiDragDropFlags.None).NativePtr != (ImGuiPayloadPtr)0)
            {
                return true;
            }

            return false;
        }
    }

    public static void ImageButtonTexture2DArray(RuntimeTexture runtimeTexture,
        Vector2 size, Vector4? bg_col = null, Vector4? tint_col = null)
    {
        ImageButtonTexture2DArray(runtimeTexture.AtlasGLTextureArrayId, runtimeTexture.IndexInAtlasTextureArray, size,
            runtimeTexture.BoundingBoxInAtlas, bg_col, tint_col);
    }

    public static void ImageButtonTexture2DArray(int texture2DArray, int indexInAtlasTextureArray,
        Vector2 size, Vector4 uvBoundingBox, Vector4? bg_col = null, Vector4? tint_col = null)
    {
        IntPtr textureId = ImGuiController.EncodeTextureArrayId(texture2DArray, indexInAtlasTextureArray);

        if (tint_col != null)
        {
            ImGui.ImageButton(textureId, size, uvBoundingBox.XW, uvBoundingBox.ZY, frame_padding: 0, bg_col.Value,
                tint_col.Value);
        }
        else if (bg_col != null)
        {
            ImGui.ImageButton(textureId, size, uvBoundingBox.XW, uvBoundingBox.ZY, frame_padding: 0, bg_col.Value);
        }
        else
        {
            ImGui.ImageButton(textureId, size, uvBoundingBox.XW, uvBoundingBox.ZY);
        }
    }


    public static void ImageTexture2DArray(RuntimeTexture runtimeTexture,
        Vector2 size, Vector4? tint_col = null, Vector4? border_col = null)
    {
        ImageTexture2DArray(runtimeTexture.AtlasGLTextureArrayId, runtimeTexture.IndexInAtlasTextureArray, size,
            runtimeTexture.BoundingBoxInAtlas, tint_col, border_col);
    }

    public static void ImageTexture2DArray(int texture2DArray, int indexInAtlasTextureArray,
        Vector2 size, Vector4 uvBoundingBox, Vector4? tint_col = null, Vector4? border_col = null)
    {
        IntPtr textureId = ImGuiController.EncodeTextureArrayId(texture2DArray, indexInAtlasTextureArray);

        if (border_col != null)
        {
            ImGui.Image(textureId, size, uvBoundingBox.XW, uvBoundingBox.ZY, tint_col.Value,
                border_col.Value);
        }
        else if (tint_col != null)
        {
            ImGui.Image(textureId, size, uvBoundingBox.XW, uvBoundingBox.ZY, tint_col.Value);
        }
        else
        {
            ImGui.Image(textureId, size, uvBoundingBox.XW, uvBoundingBox.ZY);
        }
    }

    public static void ImageTexture2D(int texture2D,
        Vector2 size, Vector4? uvBoundingBox = null, Vector4? tint_col = null, Vector4? border_col = null)
    {
        IntPtr textureId = ImGuiController.EncodeTextureId(texture2D);

        if (border_col != null)
        {
            ImGui.Image(textureId, size, uvBoundingBox.Value.XY, uvBoundingBox.Value.ZW, tint_col.Value,
                border_col.Value);
        }
        else if (tint_col != null)
        {
            ImGui.Image(textureId, size, uvBoundingBox.Value.XY, uvBoundingBox.Value.ZW, tint_col.Value);
        }
        else if (uvBoundingBox != null)
        {
            ImGui.Image(textureId, size, uvBoundingBox.Value.XY, uvBoundingBox.Value.ZW);
        }
        else
        {
            ImGui.Image(textureId, size);
        }
    }

    public static TofuImGuiSetItemSpacingGuard SetTemporaryItemSpacingForCurrentScope(float? x = null, float? y = null)
    {
        return SetTemporaryItemSpacingForCurrentScope(new Vector2(x ?? ImGui.GetStyle().ItemSpacing.X,
            y ?? ImGui.GetStyle().ItemSpacing.Y));
    }

    public static TofuImGuiSetItemSpacingGuard SetTemporaryItemSpacingForCurrentScope(Vector2 spacing)
    {
        TofuImGuiSetItemSpacingGuard guard = new TofuImGuiSetItemSpacingGuard(ImGui.GetStyle().ItemSpacing, spacing);
        return guard;
    }

    public static Vector2 GetContentRegionAvailWithPadding()
    {
        return ImGui.GetContentRegionAvail() - DefaultWindowPadding;
    }

    public const float WindowMenuBarHeight = 25;
}