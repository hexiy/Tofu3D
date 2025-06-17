using TofuEngine.UI;

namespace TofuEngine;

[RequireComponent(typeof(RectShape))]
public class Button : Component, IComponentUpdateable
{
    public delegate void MouseAction();

    private bool _clicked;
    private bool _mouseIsOver;

    [XmlIgnore]
    private MouseAction _onClickedAction;

    [LinkableComponent]
    public RectShape RectShape;

    [XmlIgnore]
    public MouseAction OnReleasedAction;

    [LinkableComponent]
    public Renderer Renderer;

    public override void Awake()
    {
        //onClickedAction += () => renderer.color = new Color(215, 125, 125);
        //onReleasedAction += () => renderer.color = Color.White;
        //onReleasedAction += SpawnCubes;

        if (GetComponent<ButtonTween>() == null)
        {
            GameObject.AddComponent<ButtonTween>().Awake();
        }

        Renderer = GetComponent<Renderer>();
        // RectShape = GetComponent<RectShape>();

        base.Awake();
    }

    public void Update()
    {
        if (Renderer == false || RectShape == false)
        {
            return;
        }

        Renderer.MousePickingEnabled = true;

        // _mouseIsOver = Tofu.MouseInput.WorldPosition.In(RectShape);
        _mouseIsOver = MousePickingSystem.HoveredRenderer == Renderer;
        if (Tofu.MouseInput.ButtonPressed() && _mouseIsOver)
        {
            _onClickedAction?.Invoke();
            _clicked = true;
        }
        else if (Tofu.MouseInput.ButtonReleased())
        {
            if (_mouseIsOver)
            {
                OnReleasedAction?.Invoke();
            }

            _clicked = false;
        }

        if (_clicked == false)
        {
            //renderer.color = mouseIsOver ? Color.Gray : Color.White;
        }

        if (_clicked && _mouseIsOver == false) // up event when me move out of button bounds, even when clicked
        {
            OnReleasedAction?.Invoke();
            _clicked = false;
        }
        //renderer.color = Color.Black;
    }
}