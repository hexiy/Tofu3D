using System.Reflection;

namespace Tofu3D;

public class InspectableData
{
	public IInspectable Inspectable { get; private set; }
	public Type InspectableType;
	public FieldOrPropertyInfo[] Infos;
	// public FieldInfo[] Fields;
	// public PropertyInfo[] Properties;

	public InspectableData(IInspectable inspectable)
	{
		Inspectable = inspectable;
		InspectableType = inspectable.GetType();
		// Fields = ComponentType.GetFields();
		// Properties = ComponentType.GetProperties();
		InitInfos();
	}

	public void InitInfos()
	{
		FieldInfo[] fields = InspectableType.GetFields();
		PropertyInfo[] properties = InspectableType.GetProperties();

		Infos = new FieldOrPropertyInfo[fields.Length + properties.Length];

		for (int fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
		{
			Infos[fieldIndex] = new FieldOrPropertyInfo(fields[fieldIndex], Inspectable);
			// Infos[fieldIndex].SetInfo(fields[fieldIndex], Component);
		}

		int offset = fields.Length;

		for (int propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++)
		{
			Infos[offset + propertyIndex] = new FieldOrPropertyInfo(properties[propertyIndex], Inspectable);

			// Infos[offset + propertyIndex].SetInfo(properties[propertyIndex], Component);
			if (properties[propertyIndex].GetValue(Inspectable) == null)
			{
				Infos[offset + propertyIndex].CanShowInEditor = false;
			}
		}


		for (int infoIndex = 0; infoIndex < Infos.Length; infoIndex++)
		{
			if (EditorPanelInspector.InspectorSupportedTypes.Contains(Infos[infoIndex].FieldOrPropertyType) == false)
			{
				Infos[infoIndex].CanShowInEditor = false;
			}
		}
	}
}