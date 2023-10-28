using System.Collections;
using System.Reflection;

namespace Tofu3D;

public class FieldOrPropertyInfo
{
    public bool CanShowInEditor;
    public bool HasSpaceAttribute;
    public string? HeaderText;
    private FieldInfo _fieldInfo;
    public bool IsReadonly;
    private PropertyInfo _propertyInfo;
    public bool IsGenericList;
    public Type GenericParameterType;
    public bool IsListElement;

    public object ListElement => _list[_index];

    private IList _list;
    private int _index = -1;

    public FieldOrPropertyInfo(IList list, int index)
    {
        _list = list;
        _index = index;
    }

    public FieldOrPropertyInfo(FieldInfo fi, object obj)
    {
        SetInfo(fi, obj);
    }

    public FieldOrPropertyInfo(PropertyInfo pi, object obj)
    {
        SetInfo(pi, obj);
    }

    // fix memory hog, dont create new infos, just update
    public void SetInfo(FieldInfo fi, object obj)
    {
        _fieldInfo = fi;
        UpdateCanShowInEditor(obj);
    }

    public void SetInfo(PropertyInfo pi, object obj)
    {
        _propertyInfo = pi;
        UpdateCanShowInEditor(obj);
    }

    public IEnumerable<CustomAttributeData> CustomAttributes
    {
        get
        {
            if (_index != -1) return ListElement.GetType().CustomAttributes;

            if (_fieldInfo != null) return _fieldInfo.CustomAttributes;

            if (_propertyInfo != null) return _propertyInfo.CustomAttributes;

            return null;
        }
    }

    public string Name
    {
        get
        {
            if (_index != -1) return "RefObject";

            if (_fieldInfo != null) return _fieldInfo.Name;

            if (_propertyInfo != null) return _propertyInfo.Name;

            return null;
        }
    }

    public Type FieldOrPropertyType
    {
        get
        {
            if (_index != -1) return ListElement.GetType();

            if (_fieldInfo != null) return _fieldInfo.FieldType;

            if (_propertyInfo != null) return _propertyInfo.PropertyType;

            return null;
        }
    }

    private void UpdateCanShowInEditor(object obj)
    {
        CanShowInEditor = true;
        if (_fieldInfo?.IsPrivate == true) CanShowInEditor = false;

        if (_fieldInfo != null && _fieldInfo.DeclaringType == typeof(Component)) CanShowInEditor = false;

        if (_propertyInfo != null && _propertyInfo?.DeclaringType == typeof(Component)) CanShowInEditor = false;

        foreach (CustomAttributeData attribute in CustomAttributes)
        {
            if (attribute.AttributeType == typeof(Show)) CanShowInEditor = true;

            if (attribute.AttributeType == typeof(Space)) HasSpaceAttribute = true;

            if (attribute.AttributeType == typeof(Header))
            {
                Type objType = obj.GetType();

                string text = attribute.ConstructorArguments[0].Value.ToString();

                HeaderText = text;
            }

            else if (attribute.AttributeType == typeof(ShowIf))
            {
                Type objType = obj.GetType();

                string name = attribute.ConstructorArguments[0].Value.ToString();

                FieldInfo field = objType.GetField(name,
                    BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
                PropertyInfo property = objType.GetProperty(name,
                    BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null) CanShowInEditor = (bool)field.GetValue(obj);

                if (property != null) CanShowInEditor = (bool)property.GetValue(obj);
            }

            else if (attribute.AttributeType == typeof(ShowIfNot))
            {
                string name = attribute.ConstructorArguments[0].Value.ToString();
                Type objType = obj.GetType();

                FieldInfo field = objType.GetField(name);
                PropertyInfo property = objType.GetProperty(name);
                if (field != null) CanShowInEditor = (bool)field.GetValue(obj) == false;

                if (property != null) CanShowInEditor = (bool)property.GetValue(obj) == false;
            }

            else if (attribute.AttributeType == typeof(Hide))
            {
                CanShowInEditor = false;
            }

            if (Global.Debug)
            {
                if (CanShowInEditor == false) IsReadonly = true;

                CanShowInEditor = true;
            }
        }
    }

    public object? GetValue(object? obj)
    {
        if (_index != -1) return ListElement;

        if (_fieldInfo != null) return _fieldInfo.GetValue(obj);

        if (_propertyInfo != null) return _propertyInfo.GetValue(obj);


        return null;
    }

    public void SetValue(object? obj, object? value)
    {
        if (_fieldInfo != null) _fieldInfo.SetValue(obj, value);

        if (_propertyInfo != null)
            if (_propertyInfo.GetSetMethod() != null)
                _propertyInfo.SetValue(obj, value);

        if (_index != -1) _list[_index] = value;
    }
}