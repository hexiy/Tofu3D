namespace TofuEngine;

public record EditorDialogParams(string message, params EditorDialogButtonDefinition[] buttons);