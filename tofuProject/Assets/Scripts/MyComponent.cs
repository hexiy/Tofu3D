using Tofu3D;
using Scripts;
using System.Collections;

[ExecuteInEditMode]
public class MyComponent : Component, IComponentUpdateable
{
    
    public override void Awake()
    {
        Tofu.CoroutineManager.StartCoroutine(MyCoroutine());
        // Tofu.CoroutineManager.StartCoroutine();
        base.Awake();
    }

    private IEnumerator MyCoroutine()
    {
        Debug.Log("after 5 seconds there should be another message");
        yield return new WaitForSeconds(5);
        Debug.Log("like this");
        yield return new WaitForSeconds(1);
        Debug.Log("and this");


    }

    public override void Start()
    {
         
        base.Start();
    }
    
    public void Update()
    {
        
    }
}