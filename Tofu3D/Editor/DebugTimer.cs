using System.Diagnostics;
using System.Linq;
using ImGuiNET;

namespace Tofu3D;

public class DebugTimer : IComparable<DebugTimer>
{
	public float[] Samples { get; private set; } = new float[200];
	public int CurrentIndex { get; private set; } = 0;
	public float Max;
	int _findMaxTriggerCounter = 10;
	public Stopwatch Stopwatch;

	public int Offset = 0;
	public string Label;
	public float? Redline;

	private int _desiredDrawOrder;
	int GroupModifiedDrawOrder
	{
		get { return _desiredDrawOrder + (int) Group; }
	}
	public bool Collapsed = false;
	public float LabelWidth { get; private set; }

	public enum SourceGroup
	{
		None = -100,
		Cpu = 0,
		Gpu = 100
	}

	public SourceGroup Group = SourceGroup.None;

	public DebugTimer(string label, SourceGroup group = SourceGroup.None, TimeSpan? redline = null, int drawOrder = 0)
	{
		LabelWidth = ImGui.CalcTextSize($"{label}:0.00 ms").X + 30;

		Group = group;
		_desiredDrawOrder = drawOrder;
		Redline = (float) (redline?.TotalMilliseconds ?? TimeSpan.FromMilliseconds(16).TotalMilliseconds);
		Label = label;
		Stopwatch = new Stopwatch();
	}

	public void AddSample(float sample)
	{
		sample = (float) MathHelper.Sin((double) Time.EditorDeltaTime) * 10;
		Samples[CurrentIndex] = sample;
		CurrentIndex++;
		_findMaxTriggerCounter--;
		if (_findMaxTriggerCounter == 0)
		{
			Max = Samples.Max();
			_findMaxTriggerCounter = 10;
		}

		Offset += 1;
		CheckForLimit();
	}

	private void CheckForLimit()
	{
		if (CurrentIndex >= Samples.Length)
		{
			CurrentIndex = 0;
		}
	}

	public int CompareTo(DebugTimer other)
	{
		return GroupModifiedDrawOrder.CompareTo(other.GroupModifiedDrawOrder);
	}
}