// [ExecuteInEditMode]

public class TestMoveSin : Component, IComponentUpdateable
{
    private int _c;
    private int _dir = 1;

    public void Update()
    {
        Transform.LocalPosition = new Vector3((float)Math.Sin(Time.EditorElapsedTime * 5) * 10, 0, 0);
        if (Time.EditorElapsedTicks % 10 == 0)
        {
            if (_c > 20 || _c < 0)
            {
                _dir *= -1;
            }

            _c += _dir;
            var x = "";
            for (var i = 0; i < _c; i++)
            {
                x += "^";
            }


            Debug.Log(x);
        }
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }
}