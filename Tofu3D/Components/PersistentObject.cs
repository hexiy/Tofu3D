namespace Tofu3D;

public class PersistentObject<T>
{
    private readonly string _name;

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
            var obj = PersistentData.Get($"PersistentObject_{_name}");
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
                var split = obj.ToString().Split(',');
                Vector3 vector = new(float.Parse(split[0].Substring(5)), float.Parse(split[1].Substring(4)),
                    float.Parse(split[2].Substring(4, split[2].Length - 5)));
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
    public static implicit operator PersistentObject<T>((string, T) tuple) => new(tuple.Item2, tuple.Item1); /*
               {
                   Value = value
               };*/

    public static implicit operator T(PersistentObject<T> obj) => obj.Value;
}