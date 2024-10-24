using System.Collections;
using System.Linq;
using System.Reflection;

namespace Tofu3D;

public class FieldOrPropertyInfo
{
    private FieldInfo _fieldInfo;
    private readonly int _index = -1;

    private readonly IList _list;
    private PropertyInfo _propertyInfo;
    public bool CanShowInEditor;
    public Type GenericParameterType;
    public bool HasSpaceAttribute;
    public string? HeaderText;
    public bool IsGenericList;
    public bool IsListElement;
    public bool IsReadonly;

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

    public object ListElement => _list[_index];

    public IEnumerable<CustomAttributeData> CustomAttributes
    {
        get
        {
            if (_index != -1)
            {
                return ListElement.GetType().CustomAttributes;
            }

            if (_fieldInfo != null)
            {
                return _fieldInfo.CustomAttributes;
            }

            if (_propertyInfo != null)
            {
                return _propertyInfo.CustomAttributes;
            }

            return null;
        }
    }

    public string Name
    {
        get
        {
            if (_index != -1)
            {
                return "RefObject";
            }

            if (_fieldInfo != null)
            {
                return _fieldInfo.Name;
            }

            if (_propertyInfo != null)
            {
                return _propertyInfo.Name;
            }

            return null;
        }
    }

    public Type FieldOrPropertyType
    {
        get
        {
            if (_index != -1)
            {
                return ListElement.GetType();
            }

            if (_fieldInfo != null)
            {
                return _fieldInfo.FieldType;
            }

            if (_propertyInfo != null)
            {
                return _propertyInfo.PropertyType;
            }

            return null;
        }
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

    private void UpdateCanShowInEditor(object obj)
    {
        CanShowInEditor = true;
        if (_fieldInfo?.IsPrivate == true)
        {
            CanShowInEditor = false;
        }

        if (_fieldInfo != null && _fieldInfo.DeclaringType == typeof(Component))
        {
            CanShowInEditor = false;
        }


        // if property is private, dont show it
        if (_propertyInfo?.GetAccessors(true).Any(a => a.IsPrivate) == true)
        {
            CanShowInEditor = false;
        }

        if (_propertyInfo != null && _propertyInfo?.DeclaringType == typeof(Component))
        {
            CanShowInEditor = false;
        }

        foreach (var attribute in CustomAttributes)
        {
            if (attribute.AttributeType == typeof(Show))
            {
                CanShowInEditor = true;
            }

            if (attribute.AttributeType == typeof(Space))
            {
                HasSpaceAttribute = true;
            }

            if (attribute.AttributeType == typeof(Header))
            {
                var objType = obj.GetType();

                var text = attribute.ConstructorArguments[0].Value.ToString();

                HeaderText = text;
            }

            else if (attribute.AttributeType == typeof(ShowIf))
            {
                var objType = obj.GetType();

                var name = attribute.ConstructorArguments[0].Value.ToString();

                var field = objType.GetField(name,
                    BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
                var property = objType.GetProperty(name,
                    BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    CanShowInEditor = (bool)field.GetValue(obj);
                }

                if (property != null)
                {
                    CanShowInEditor = (bool)property.GetValue(obj);
                }
            }

            else if (attribute.AttributeType == typeof(ShowIfNot))
            {
                var name = attribute.ConstructorArguments[0].Value.ToString();
                var objType = obj.GetType();

                var field = objType.GetField(name);
                var property = objType.GetProperty(name);
                if (field != null)
                {
                    CanShowInEditor = (bool)field.GetValue(obj) == false;
                }

                if (property != null)
                {
                    CanShowInEditor = (bool)property.GetValue(obj) == false;
                }
            }

            else if (attribute.AttributeType == typeof(Hide))
            {
                CanShowInEditor = false;
            }

            if (Global.Debug)
            {
                if (CanShowInEditor == false)
                {
                    IsReadonly = true;
                }

                CanShowInEditor = true;
            }
        }
    }

    public object? GetValue(object? obj)
    {
        if (_index != -1)
        {
            return ListElement;
        }

        if (_fieldInfo != null)
        {
            return _fieldInfo.GetValue(obj);
        }

        if (_propertyInfo != null)
        {
            return _propertyInfo.GetValue(obj);
        }


        return null;
    }

    public void SetValue(object? obj, object? value)
    {
        if (_fieldInfo != null)
        {
            _fieldInfo.SetValue(obj, value);
            EditorPanelInspector.I.OnAnyValueChanged();
        }

        if (_propertyInfo != null)
        {
            if (_propertyInfo.GetSetMethod() != null)
            {
                _propertyInfo.SetValue(obj, value);
                EditorPanelInspector.I.OnAnyValueChanged();
            }
        }

        if (_index != -1)
        {
            _list[_index] = value;
        }
    }
}