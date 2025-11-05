using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace TofuEngine;

[ExecuteInEditMode]
public class TerrainGenerator : Component, IComponentUpdateable
{
    private readonly ConcurrentQueue<GameObject> _concurrentBag = new ConcurrentQueue<GameObject>();
    private readonly float _cubeModelSize = 2;

    private int _threadsWorkingCount = -1;
    private GameObject? _cubePrefab;

    [XmlIgnore]
    public Action Despawn;

    [XmlIgnore]
    public Action Spawn;

    public int TerrainSize = 10;
    public int ThreadsToUse = 5;

    private Asset_Material? _grassMaterial = null;
    private Asset_Material? _waterMaterial = null;

    public override void Awake()
    {
        Spawn += StartTerrainGenerationOnNewThread;
        Despawn += DestroyTerrain;

        base.Awake();
    }

    /*public override void Start()
    {
        Spawn.Invoke();
        base.Start();
    }*/

    private void CreateCubePrefab()
    {
        _cubePrefab = GameObject.Create(name: "cube", runtimeOnly: true, visibleInHierarchy: false);
        _cubePrefab.AddComponent<BoxShape>();
        ModelRenderer modelRenderer = _cubePrefab.AddComponent<ModelRenderer>();
        modelRenderer.NeedsToSetupMesh = false;

        _cubePrefab.Awake();

        string modelPath = Path.Combine("3D", "Minecraft_Grass_Block_OBJ", "Grass_Block.obj");
        Asset_Model model = Tofu.AssetLoadManager.Get<Asset_Model>(modelPath);
        modelRenderer.RuntimeMesh = model.GetMesh(0);

        string texturePath = Path.Combine("3D", "Minecraft_Grass_Block_OBJ", "Grass_Block_TEX.png");

        modelRenderer.Material.AlbedoTexture = Tofu.AssetLoadManager.Get<RuntimeTexture>(texturePath);
        modelRenderer.Material.MetallicTextureStrength = 0.2f;
        modelRenderer.Material.Smoothness = 0f;
        if (_grassMaterial == null)
        {
            _grassMaterial = Tofu.AssetLoadManager.CreateCopyFile<Asset_Material>(modelRenderer.Material,
                folder: Folders.MaterialsInAssets);

            _waterMaterial = Tofu.AssetLoadManager.CreateCopyFile<Asset_Material>(modelRenderer.Material,
                folder: Folders.MaterialsInAssets);

            _waterMaterial.AlbedoTexture = Tofu.Editor.EditorTextures.WhitePixel;
            _waterMaterial.AlbedoColor = Color.Blue;
            _waterMaterial.AlbedoColor.SetAlpha(0.2f);

            _waterMaterial.MaterialType = MaterialType.Unlit;
            _waterMaterial.RenderMode = RenderMode.Transparent;
        }
    }

    public void Update()
    {
        if (_threadsWorkingCount == 0)
        {
            _threadsWorkingCount = -1;
            AddBlocksToScene();
        }

        if (KeyboardInput.WasKeyJustPressed(Keys.Space))
        {
            Spawn?.Invoke();
        }
    }


    private void DestroyTerrain()
    {
        for (int i = 0; i < Transform.Children.Count; i++)
        {
            Transform.Children[0].GameObject.Destroy();
        }

        Transform.Children = new List<Transform>();
    }

    private void StartTerrainGenerationOnNewThread()
    {
        if (_cubePrefab == null)
        {
            CreateCubePrefab();
        }

        _cubePrefab.SetActive(true);

        Tofu.SceneSerializer.SaveClipboardGameObject(_cubePrefab);

        DestroyTerrain();
        _concurrentBag.Clear();

        Debug.StartTimer(
            $"TerrainGeneration {TerrainSize}x{TerrainSize} - Total of {TerrainSize * TerrainSize} blocks");

        int numberOfThreads = ThreadsToUse;
        _threadsWorkingCount = numberOfThreads;
        List<Thread> threads = new List<Thread>();
        for (int threadIndex = 0; threadIndex < numberOfThreads; threadIndex++)
        {
            int capturedThreadIndex = threadIndex;
            Thread thread = new Thread(() =>
                GenerateTerrain(TerrainSize, _cubePrefab, capturedThreadIndex, numberOfThreads));
            threads.Add(thread);
        }

        threads.ForEach(t => t.Start());
    }

    private void StartTerrainGenerationOnMainThread()
    {
        if (_cubePrefab == null)
        {
            return;
        }

        Tofu.SceneSerializer.SaveClipboardGameObject(_cubePrefab);

        DestroyTerrain();
        _concurrentBag.Clear();

        GenerateTerrain(TerrainSize, _cubePrefab, 0, 1);
    }

    private void GenerateTerrain(int terrainSize, GameObject referenceGameObject, int threadIndex, int numberOfThreads)
    {
        Debug.StartTimer($"Thread #{threadIndex} finished");

        int totalBlocks = terrainSize * terrainSize;
        int blocksPerThread = totalBlocks / numberOfThreads;
        int startIndex = blocksPerThread * threadIndex;
        int endIndex = blocksPerThread + threadIndex * blocksPerThread;


        for (int i = startIndex; i < endIndex; i++)
        {
            // Debug.Log(i);
            GameObject go = (GameObject)referenceGameObject.Clone(false);
            go.Name = $"Thread:{threadIndex} go {i}";
            go.RuntimeOnly = true;

            _concurrentBag.Enqueue(go);
        }


        Debug.EndAndLogTimer($"Thread #{threadIndex} finished");

        _threadsWorkingCount--;
        if (_threadsWorkingCount == 0)
        {
            // AddBlocksToScene();
        }
    }

    private void AddBlocksToScene()
    {
        int x = 0;
        int z = 0;

        Tofu.SceneManager.CurrentScene.AddGameObjectsToScene(_concurrentBag);
        foreach (GameObject go in _concurrentBag)
        {
            go.Transform.SetParent(Transform);

            float positionY = Mathf.Sin(x / 10f) * Mathf.Cos((float)z / 10) * 15;
            bool isWater = positionY < -1;
            if (isWater)
            {
                go.GetComponent<Renderer>().Material = _waterMaterial;
            }

            if (positionY < -1)
            {
                positionY = 0;
            }


            positionY = positionY.TranslateToGrid(2);

            go.Transform.LocalPosition = new Vector3(x * _cubeModelSize, positionY, z * _cubeModelSize);
            go.SetActive(true);
            x++;
            if (x > TerrainSize)
            {
                x = 0;
                z++;
            }
        }

        Debug.EndAndLogTimer(
            $"TerrainGeneration {TerrainSize}x{TerrainSize} - Total of {TerrainSize * TerrainSize} blocks");

        _cubePrefab.SetActive(false);
    }

    private void LongTask()
    {
        List<GameObject> gameObjects = new List<GameObject>(20000);
        for (int i = 0; i < 20000; i++)
        {
            GameObject go = GameObject.Create(name: i.ToString(), addToScene: false);
            gameObjects.Add(go);
            Debug.Log(i);
        }

        lock (Tofu.SceneManager.CurrentScene.GameObjects)
        {
            Tofu.SceneManager.CurrentScene.AddGameObjectsToScene(gameObjects);
        }
    }
}