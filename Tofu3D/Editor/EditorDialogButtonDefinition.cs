namespace Tofu3D;

public record EditorDialogButtonDefinition(string text, Action clicked, bool closeOnClick = false);