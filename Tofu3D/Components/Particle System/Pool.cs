using System.Collections.Concurrent;

namespace Tofu3D;

public class Pool<T>
{
    private readonly ConcurrentBag<T> _collection = new();
    private readonly Func<T> _objectGenerator;

    public Pool(Func<T> generator)
    {
        if (generator == null)
        {
            throw new ArgumentNullException("objectGenerator");
        }

        _collection = new ConcurrentBag<T>();
        _objectGenerator = generator;
    }

    public int Count => _collection.Count;

    public void PutObject(T item)
    {
        _collection.Add(item);
    }

    public T GetObject()
    {
        T item;
        if (_collection.TryTake(out item))
        {
            return item;
        }

        return _objectGenerator();
    }
}