public class Flame : Component
{
    public Rigidbody Rb;

    public override void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        base.Awake();
    }
}