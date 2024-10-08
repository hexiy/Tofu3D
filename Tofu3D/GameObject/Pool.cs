namespace Scripts;

public class Pool
{
    public Stack<GameObject> FreeObjects = new();
    public GameObject Go;
    public Stack<GameObject> UsedObjects = new();

    private void AddNewObject()
    {
        var gameObject = GameObject.Create(name: "Pooled object");
        for (var i = 0; i < Go.Components.Count; i++)
        {
            gameObject.AddComponent(Go.Components[i].GetType());
        }

        gameObject.Awake();
        FreeObjects.Push(gameObject);
    }

    public GameObject Request()
    {
        if (FreeObjects.Count == 0)
        {
            AddNewObject();
        }

        var gameObject = FreeObjects.Pop();
        gameObject.SetActive(true);
        UsedObjects.Push(gameObject);
        return gameObject;
    }

    public void Return(GameObject gameObject)
    {
        gameObject.SetActive(false);
        FreeObjects.Push(gameObject);
    }
}