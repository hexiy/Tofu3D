using System.Linq;
using System.Reflection;

namespace Tofu3D;

public class InspectableData
{
    public FieldOrPropertyInfo[] Infos;

    public Type InspectableType;
    // public FieldInfo[] Fields;
    // public PropertyInfo[] Properties;

    public object Inspectable { get; }
    public Inspector Inspector;

    public InspectableData(object inspectable, Inspector inspector)
    {
        if (inspectable == null)
        {
            throw new ArgumentNullException(nameof(inspectable));
        }

        Inspector = inspector;
        Inspectable = inspectable;
        InspectableType = inspectable.GetType();
        // Fields = ComponentType.GetFields();
        // Properties = ComponentType.GetProperties();
        InitInfos();
    }

    public void InitInfos()
    {
        List<MemberInfo> members = InspectableType
            .FindMembers(MemberTypes.Field | MemberTypes.Property,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                null).ToList();

        Infos = new FieldOrPropertyInfo[members.Count];
        for (int i = 0; i < members.Count; i++)
        {
            MemberInfo memberInfo = members[i];
            if (memberInfo.MemberType is MemberTypes.Field)
            {
                Infos[i] = new FieldOrPropertyInfo((FieldInfo)memberInfo, Inspectable, this);
            }
            else
            {
                Infos[i] = new FieldOrPropertyInfo((PropertyInfo)memberInfo, Inspectable, this);
                if (Infos[i].GetValue(Inspectable) == null)
                {
                    Infos[i].CanShowInEditor = false;
                }
            }
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

                if (Inspector.InspectorSupportedTypes.Contains(genericType) == false)
                {
                    Infos[infoIndex].CanShowInEditor = false;
                }

                continue;
            }

            if (Inspector.InspectorSupportedTypes.Contains(Infos[infoIndex].FieldOrPropertyType) == false
                && Infos[infoIndex].FieldOrPropertyType.BaseType != typeof(Enum))
            {
                Infos[infoIndex].CanShowInEditor = false;
            }
        }
    }
}