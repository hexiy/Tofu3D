using System.Collections;
using System.Linq;
using System.Reflection;

namespace TofuEngine;

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
    public string? NameOverride;
    public bool IsGenericList;
    public bool IsListElement;
    public bool IsReadonly;
    private InspectableData _inspectableData;
    public object? AdditionalData;

    public FieldOrPropertyInfo(IList list, int index, InspectableData inspectableData)
    {
        _list = list;
        _index = index;
        _inspectableData = inspectableData;
    }

    public FieldOrPropertyInfo(FieldInfo fi, object obj, InspectableData inspectableData)
    {
        SetInfo(fi, obj);
        _inspectableData = inspectableData;
    }

    public FieldOrPropertyInfo(PropertyInfo pi, object obj, InspectableData inspectableData)
    {
        SetInfo(pi, obj);
        _inspectableData = inspectableData;
    }

    public object ListElement => _list[_index];

    public bool GetCustomAttribute<T>(out T? attribute) where T : Attribute
    {
        attribute = GetCustomAttribute<T>();
        return attribute != null;
    }

    public bool HasCustomAttribute<T>() where T : Attribute
    {
        return GetCustomAttribute<T>() != null;
    }

    public T? GetCustomAttribute<T>() where T : Attribute
    {
        if (_fieldInfo != null)
        {
            return _fieldInfo.GetCustomAttributes<T>().FirstOrDefault(attrib => attrib.GetType() == typeof(T), null);
        }

        if (_propertyInfo != null)
        {
            // return _propertyInfo.GetCustomAttribute<T>();
            return _propertyInfo.GetCustomAttributes<T>().FirstOrDefault(attrib => attrib.GetType() == typeof(T), null);

        }

        return null;
    }

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

            if (NameOverride != null)
            {
                return NameOverride;
            }

            string name = "";
            if (_fieldInfo != null)
            {
                name = _fieldInfo.Name;
            }

            if (_propertyInfo != null)
            {
                name = _propertyInfo.Name;
            }

            if (HasCustomAttribute<SplitWords>())
            {
                name = StringExtensions.SplitCamelCase(name);
            }

            return name;
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
        Init(obj);
    }

    public void SetInfo(PropertyInfo pi, object obj)
    {
        _propertyInfo = pi;
        Init(obj);
    }

    private void Init(object obj)
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


        if (HasCustomAttribute<Show>())
        {
            CanShowInEditor = true;
        }

        if (HasCustomAttribute<Space>())
        {
            HasSpaceAttribute = true;
        }


        if (GetCustomAttribute<InspectorNameOverride>(out InspectorNameOverride nameOverrideAttrib))
        {
            NameOverride = nameOverrideAttrib.Name;
        }

        if (GetCustomAttribute<Header>(out Header headerAttrib))
        {
            string? text = headerAttrib.Text;

            HeaderText = text;
        }

        if (GetCustomAttribute<ShowIf>(out ShowIf showIfAttrib))
        {
            Type objType = obj.GetType();

            string? name = showIfAttrib.FieldName;

            FieldInfo? field = objType.GetField(name,
                BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic);
            PropertyInfo? property = objType.GetProperty(name,
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

        if (GetCustomAttribute<ShowIfNot>(out ShowIfNot showIfNotAttrib))
        {
            string? name = showIfNotAttrib.FieldName;
            Type objType = obj.GetType();

            FieldInfo? field = objType.GetField(name);
            PropertyInfo? property = objType.GetProperty(name);
            if (field != null)
            {
                CanShowInEditor = (bool)field.GetValue(obj) == false;
            }

            if (property != null)
            {
                CanShowInEditor = (bool)property.GetValue(obj) == false;
            }
        }

        if (HasCustomAttribute<Hide>())
        {
            CanShowInEditor = false;
        }

        if (HasCustomAttribute<ReadOnly>())
        {
            IsReadonly = true;
        }

        // Show everything in debug mode, if it was not shown before, set it to readonly
        if (Global.Debug)
        {
            if (CanShowInEditor == false)
            {
                IsReadonly = true;
            }

            CanShowInEditor = true;
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
            Tofu.Editor.AfterDraw += () =>
            {
                _inspectableData.Inspector.FieldChangedByUserInspectableCallback?.Invoke(_fieldInfo.Name);
                _inspectableData.Inspector.FieldChangedByUser.Invoke(_fieldInfo.Name);
            };
        }

        if (_propertyInfo != null)
        {
            if (_propertyInfo.GetSetMethod() != null)
            {
                _propertyInfo.SetValue(obj, value);
                Tofu.Editor.AfterDraw += () =>
                {
                    _inspectableData.Inspector.FieldChangedByUserInspectableCallback?.Invoke(_propertyInfo.Name);
                    _inspectableData.Inspector.FieldChangedByUser.Invoke(_propertyInfo.Name);
                };
            }
        }

        if (_index != -1)
        {
            _list[_index] = value;
        }
    }
}