using System.Reflection;

namespace Tofu3D;

public class ComponentInspectorData
{
	public Component Component;
	public Type ComponentType;
	public FieldOrPropertyInfo[] Infos;
	// public FieldInfo[] Fields;
	// public PropertyInfo[] Properties;

	public ComponentInspectorData(Component component)
	{
		Component = component;
		ComponentType = component.GetType();
		// Fields = ComponentType.GetFields();
		// Properties = ComponentType.GetProperties();
		InitInfos();
	}

	private void InitInfos()
	{
		FieldInfo[] fields = ComponentType.GetFields();
		PropertyInfo[] properties = ComponentType.GetProperties();

		Infos = new FieldOrPropertyInfo[fields.Length + properties.Length];

		for (int fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
		{
			Infos[fieldIndex] = new FieldOrPropertyInfo(fields[fieldIndex], Component);
			// Infos[fieldIndex].SetInfo(fields[fieldIndex], Component);
		}

		int offset = fields.Length;

		for (int propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++)
		{
			Infos[offset + propertyIndex] = new FieldOrPropertyInfo(properties[propertyIndex], Component);

			// Infos[offset + propertyIndex].SetInfo(properties[propertyIndex], Component);
			if (properties[propertyIndex].GetValue(Component) == null)
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