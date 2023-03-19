namespace Tofu3D;

[Flags]
public enum LogCategory
{
	Info = 1 << 0,
	Warning = 1 << 1,
	Error = 1 << 2,
	Timer = 1 << 3,
}