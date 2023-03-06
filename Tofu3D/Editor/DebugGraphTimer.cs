using System.Diagnostics;
using System.Linq;
using ImGuiNET;

namespace Tofu3D;

public class DebugGraphTimer : IComparable<DebugGraphTimer>
{
	public float[] Samples { get; private set; } = new float[200];
	public int CurrentIndex { get; private set; } = 0;

	float _foundMaxSample;
	float _foundMinSample;
	public float MaxSample;
	public float MinSample;
	int _findMaxTriggerFrameCounter = 0;
	public float Sample10FramesAgo = 0;
	public Stopwatch Stopwatch;

	public int Offset = 0;
	public string Label;
	public float? Redline;

	private int _desiredDrawOrder;
	int GroupModifiedDrawOrder
	{
		get { return _desiredDrawOrder + (int) Group; }
	}
	public bool Collapsed
	{
		get { return PersistentData.GetBool($"DebugTimerCollapsed_{Label}", false); }
		set { PersistentData.Set($"DebugTimerCollapsed_{Label}", value); }
	}
	public float LabelWidth { get; private set; }

	public enum SourceGroup
	{
		None = -100,
		Update = 0,
		Render = 100
	}

	public SourceGroup Group = SourceGroup.None;

	public DebugGraphTimer(string label, SourceGroup group = SourceGroup.None, TimeSpan? redline = null, int drawOrder = 0)
	{
		LabelWidth = ImGui.CalcTextSize($"{label}:0.00 ms").X + 30;

		Group = group;
		_desiredDrawOrder = drawOrder;
		Redline = (float) (redline?.TotalMilliseconds ?? TimeSpan.FromMilliseconds(16).TotalMilliseconds);
		Label = label;
		Stopwatch = new Stopwatch();
	}

	public void SetSamplesBufferSize(int bufferSize)
	{
		Samples = new float[bufferSize];
	}

	public void AddSample(float sample)
	{
		CheckForLimit();

		Samples[CurrentIndex] = sample;
		CurrentIndex++;
		_findMaxTriggerFrameCounter--;
		if (_findMaxTriggerFrameCounter < 0)
		{
			_foundMaxSample = Samples.Max();
			_foundMinSample = Samples.Min();
			_findMaxTriggerFrameCounter = 10;
			Sample10FramesAgo = (float) Math.Round(sample, 2);
		}

		MaxSample = Mathf.Lerp(MaxSample, _foundMaxSample, Time.EditorDeltaTime * 7);
		MinSample = Mathf.Lerp(MinSample, _foundMinSample, Time.EditorDeltaTime * 7);

		Offset += 1;
	}

	private void CheckForLimit()
	{
		if (CurrentIndex >= Samples.Length)
		{
			CurrentIndex = 0;
		}
	}

	public int CompareTo(DebugGraphTimer other)
	{
		return GroupModifiedDrawOrder.CompareTo(other.GroupModifiedDrawOrder);
	}
}