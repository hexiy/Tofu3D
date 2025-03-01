using TofuEngine;
using Scripts;
using System.Collections;

[ExecuteInEditMode]
public class MyComponent : Component, IComponentUpdateable
{
    public override void Awake()
    {
        StartCoroutine(MyCoroutine());
        base.Awake();
    }

    private IEnumerator MyCoroutine()
    {
        Debug.Log("after 3 seconds there should be another message");
        yield return new WaitForSeconds(3);
        Debug.Log("like this, now waiting for space key press");
        yield return new WaitWhile(() => KeyboardInput.IsKeyDown(Keys.Space) == false);
        Debug.Log("space pressed");
        yield return null;
        Debug.Log("after yield return null");

    }

    public override void Start()
    {
        base.Start();
    }

    public void Update()
    {
    }
}