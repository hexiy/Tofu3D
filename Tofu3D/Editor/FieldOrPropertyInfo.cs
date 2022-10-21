using System.Linq;
using System.Reflection;

namespace Tofu3D;

public class FieldOrPropertyInfo
{
	public bool CanShowInEditor = true;
	FieldInfo _fieldInfo;
	public bool IsReadonly;
	PropertyInfo _propertyInfo;

	public FieldOrPropertyInfo(FieldInfo fi, object obj)
	{
		_fieldInfo = fi;
		UpdateCanShowInEditor(obj);
	}

	public FieldOrPropertyInfo(PropertyInfo pi, object obj)
	{
		_propertyInfo = pi;
		UpdateCanShowInEditor(obj);
	}

	public IEnumerable<CustomAttributeData> CustomAttributes
	{
		get
		{
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

	void UpdateCanShowInEditor(object obj)
	{
		if (_fieldInfo != null && _fieldInfo.DeclaringType == typeof(Component))
		{
			CanShowInEditor = false;
		}

		if (_propertyInfo != null && _propertyInfo?.DeclaringType == typeof(Component))
		{
			CanShowInEditor = false;
		}

		for (int i = 0; i < CustomAttributes.Count(); i++)
		{
			if (CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(Show))
			{
				CanShowInEditor = true;
			}

			if (CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(ShowIf))
			{
				string name = CustomAttributes.ElementAtOrDefault(i).ConstructorArguments[0].Value.ToString();

				FieldInfo field = obj.GetType().GetField(name);
				PropertyInfo property = obj.GetType().GetProperty(name);
				if (field != null)
				{
					CanShowInEditor = (bool) field.GetValue(obj);
				}

				if (property != null)
				{
					CanShowInEditor = (bool) property.GetValue(obj);
				}
			}

			if (CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(ShowIfNot))
			{
				string name = CustomAttributes.ElementAtOrDefault(i).ConstructorArguments[0].Value.ToString();

				FieldInfo field = obj.GetType().GetField(name);
				PropertyInfo property = obj.GetType().GetProperty(name);
				if (field != null)
				{
					CanShowInEditor = (bool) field.GetValue(obj) == false;
				}

				if (property != null)
				{
					CanShowInEditor = (bool) property.GetValue(obj) == false;
				}
			}

			if (CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(Hide))
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
		}

		if (_propertyInfo != null)
		{
			if (_propertyInfo.GetSetMethod() != null)
			{
				_propertyInfo.SetValue(obj, value);
			}
		}
	}
}