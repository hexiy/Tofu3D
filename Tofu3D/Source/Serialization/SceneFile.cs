namespace TofuEngine;

public struct SceneFile
{
    public List<GameObject> GameObjects;
    public List<Component> Components;
    public int GameObjectNextId;
    

    public static SceneFile CreateForOneGameObject(GameObject go)
    {
        SceneFile sceneFile = new SceneFile
        {
            GameObjects = [],
            Components = []
        };
        sceneFile.GameObjects.Add(go);
        sceneFile.Components.AddRange(go.Components);

        for (int i = 0; i < go.Transform.Children.Count; i++)
        {
            sceneFile.GameObjects.Add(go.Transform.Children[i].GameObject);
            sceneFile.Components.AddRange(go.Transform.Children[i].GameObject.Components);
        }

        //return new SceneFile() { GameObjects = new List<GameObject>() { go }, Components = go.components };
        return sceneFile;
    }
}