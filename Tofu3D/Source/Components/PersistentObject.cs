namespace TofuEngine;

public class PersistentObject<T>
{
    private readonly string _name;
    private readonly T? _defaultValue;

    public PersistentObject(string name, T? defaultValue = default)
    {
        _name = name;
        _defaultValue = defaultValue;
    }

    private PersistentObject(T assignValue, string name)
    {
        // if (Id == 0)
        // {
        // Id = (uint) Rendom.Range(0, uint.MaxValue);
        if (_name == null)
        {
            _name = name;

            if (PersistentData.Get($"PersistentObject_{_name}", () => _defaultValue) ==
                null)
            {
                Value = assignValue; // only assign default value if this persistent object isnt initialized
            }
        }
        // }
    }

    public T? Value
    {
        get
        {
            object? obj = PersistentData.Get($"PersistentObject_{_name}", ()=>_defaultValue);
            if (obj == null)
            {
                return default;
            }

            if (typeof(T).IsEnum)
            {
                return (T)Enum.Parse(typeof(T), obj.ToString());
            }

            // return (T) Enum.ToObject(typeof(T), obj);
            if (typeof(T) == typeof(Vector3))
            {
                string[] split = obj.ToString()
                    .Replace("\n", string.Empty)
                    .Replace("{", string.Empty)
                    .Replace("}", string.Empty)
                    .Split(',');
                Vector3 vector = new Vector3(float.Parse(split[0].Substring(6)),
                    float.Parse(split[1].Substring(6)),
                    float.Parse(split[2].Substring(6)));
                return (T)Convert.ChangeType(vector, typeof(T));
            }

            return (T)Convert.ChangeType(obj, typeof(T));
        }
        set => PersistentData.Set($"PersistentObject_{_name}", value);
    }

    /*public static implicit operator PersistentObject<T>(T value)
    {
        return new PersistentObject<T>(value); /*
               {
                   Value = value
               };#1#
    }*/
    public static implicit operator PersistentObject<T>((string, T) tuple) =>
        new PersistentObject<T>(tuple.Item2, tuple.Item1); /*
                   {
                       Value = value
                   };*/

    public static implicit operator T(PersistentObject<T> obj) => obj.Value;
}