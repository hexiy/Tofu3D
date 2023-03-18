namespace Tofu3D;

[Flags]
public enum LogCategoryFilter
{
	None = 0,
	Message = 1 << 0,
	Warning = 1 << 1,
	Error = 1 << 2,
	Timer = 1 << 3,
	All = Message | Warning | Error | Timer,
}