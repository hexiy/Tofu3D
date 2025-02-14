namespace TofuEngine;

public record EditorDialogButtonDefinition(string text, Action clicked, bool closeOnClick = false);