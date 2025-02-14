public class CodeEditorInfo
{
    public string Name { get; set; } = "";
    public string[] ExecutableNames { get; set; } = Array.Empty<string>();
    // public string PathToExecutable { get; set; }
    public bool IsValid { get; set; } = false;
    public bool IsAddedByUser { get; set; } = false;
    public string ArgsTemplate { get; set; } = "";
}