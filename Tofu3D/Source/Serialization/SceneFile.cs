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
            foreach (Transform child in go.Transform.Children)
            {
                sceneFile.GameObjects.Add(child.GameObject);
                sceneFile.Components.AddRange(child.GameObject.Components);
            }
        }

        //return new SceneFile() { GameObjects = new List<GameObject>() { go }, Components = go.components };
        return sceneFile;
    }
}