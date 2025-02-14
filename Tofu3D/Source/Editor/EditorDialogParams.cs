namespace Tofu3D;

public record EditorDialogParams(string message, params EditorDialogButtonDefinition[] buttons);