namespace Tofu3D;

public class PersistentObject<T>
{
    private string _name;

    public T? Value
    {
        get
        {
            object obj = PersistentData.Get($"PersistentObject_{_name}", default);
            if (obj == null) return (T?)default;
            if (typeof(T).IsEnum) return (T)Enum.Parse(typeof(T), obj.ToString());
            // return (T) Enum.ToObject(typeof(T), obj);
            if (typeof(T) == typeof(Vector3))
            {
                string[] split = obj.ToString().Split();
                Vector3 vector = new(float.Parse(split[0].Substring(1)), float.Parse(split[2]),
                    float.Parse(split[4].Substring(0, split[4].Length - 1)));
                return (T)Convert.ChangeType(vector, typeof(T));
            }

            return (T)Convert.ChangeType(obj, typeof(T));
        }
        set => PersistentData.Set($"PersistentObject_{_name}", value);
    }

    public PersistentObject(string name)
    {
        _name = name;
    }

    private PersistentObject(T assignValue, string name)
    {
        // if (Id == 0)
        // {
        // Id = (uint) Rendom.Range(0, uint.MaxValue);
        if (_name == null)
        {
            _name = name;

            if (PersistentData.Get($"PersistentObject_{_name}") ==
                null) Value = assignValue; // only assign default value if this persistent object isnt initialized
        }
        // }
    }

    /*public static implicit operator PersistentObject<T>(T value)
    {
        return new PersistentObject<T>(value); /*
               {
                   Value = value
               };#1#
    }*/
    public static implicit operator PersistentObject<T>((string, T) tuple)
    {
        return new PersistentObject<T>(tuple.Item2, tuple.Item1); /*
               {
                   Value = value
               };*/
    }

    public static implicit operator T(PersistentObject<T> obj)
    {
        return obj.Value;
    }
}