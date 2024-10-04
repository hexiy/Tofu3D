namespace Scripts;

public class CharacterController : Component, IComponentUpdateable
{
    private bool _jumpKeyDown;

    // LINKABLECOMPONENT PURGE //[LinkableComponent]
    private Rigidbody _rb;
    public float JumpForce = 10000;
    public float MoveSpeed = 10;

    public void Update()
    {
        if (_rb == null)
        {
            return;
        }

        var input = Vector2.Zero;
        if (KeyboardInput.IsKeyDown(Keys.A))
        {
            input.X = -MoveSpeed;
        }

        if (KeyboardInput.IsKeyDown(Keys.D))
        {
            input.X = MoveSpeed;
        }

        if (_jumpKeyDown == false && KeyboardInput.IsKeyDown(Keys.W))
        {
            //rb.body.ApplyForce(new Vector2(0, -JumpForce));
        }

        _jumpKeyDown = KeyboardInput.IsKeyDown(Keys.W);
        //rb.body.ApplyForce(new Vector2(input.X, 0));    }
    }

    public override void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        base.Awake();
    }
}