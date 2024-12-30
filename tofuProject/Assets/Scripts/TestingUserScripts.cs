using Tofu3D;

namespace Scripts;

public class TestingUserScripts : Component, IComponentUpdateable
{
    private Text _text;

    public void Update()
    {
        // Debug.Log("x");
        if(_text==null){
            _text=GetComponent<Text>();
        }
        if(_text==null){return;}
        
        _text.Value = "y";
        
    }
}

