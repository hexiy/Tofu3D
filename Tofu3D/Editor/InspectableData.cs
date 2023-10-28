using System.Linq;
using System.Reflection;

namespace Tofu3D;

public class InspectableData
{
    public object Inspectable { get; private set; }
    public Type InspectableType;

    public FieldOrPropertyInfo[] Infos;
    // public FieldInfo[] Fields;
    // public PropertyInfo[] Properties;

    public InspectableData(object inspectable)
    {
        Inspectable = inspectable;
        InspectableType = inspectable.GetType();
        // Fields = ComponentType.GetFields();
        // Properties = ComponentType.GetProperties();
        InitInfos();
    }

    public void InitInfos()
    {
        FieldInfo[] fields =
            InspectableType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        PropertyInfo[] properties = InspectableType.GetProperties();

        Infos = new FieldOrPropertyInfo[fields.Length + properties.Length];

        for (int fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
            Infos[fieldIndex] = new FieldOrPropertyInfo(fields[fieldIndex], Inspectable);
        // Infos[fieldIndex].SetInfo(fields[fieldIndex], Component);
        int offset = fields.Length;

        for (int propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++)
        {
            Infos[offset + propertyIndex] = new FieldOrPropertyInfo(properties[propertyIndex], Inspectable);

            // Infos[offset + propertyIndex].SetInfo(properties[propertyIndex], Component);
            if (properties[propertyIndex].GetValue(Inspectable) == null)
                Infos[offset + propertyIndex].CanShowInEditor = false;
        }


        for (int infoIndex = 0; infoIndex < Infos.Length; infoIndex++)
        {
            //== "List`1")
            if (Infos[infoIndex].FieldOrPropertyType.IsGenericType &&
                Infos[infoIndex].FieldOrPropertyType.Name
                    .Contains("List`1")) // && Infos[infoIndex].FieldOrPropertyType.GetGenericTypeDefinition() == typeof(IList<>))
            {
                Type genericType = Infos[infoIndex].FieldOrPropertyType.GenericTypeArguments.First();
                Infos[infoIndex].IsGenericList = true;
                Infos[infoIndex].GenericParameterType = genericType;

                if (EditorPanelInspector.InspectorSupportedTypes.Contains(genericType) == false)
                    Infos[infoIndex].CanShowInEditor = false;

                continue;
            }

            if (EditorPanelInspector.InspectorSupportedTypes.Contains(Infos[infoIndex].FieldOrPropertyType) == false
                && Infos[infoIndex].FieldOrPropertyType.BaseType != typeof(Enum))
                Infos[infoIndex].CanShowInEditor = false;
        }
    }
}