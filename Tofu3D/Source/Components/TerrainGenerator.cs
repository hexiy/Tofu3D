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
    private GameObject? _grassPrefab;
    private GameObject? _waterPrefab;

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

        // GameObject.IsStaticSelf = true;
        base.Awake();
    }

    /*public override void Start()
    {
        Spawn.Invoke();
        base.Start();
    }*/

    private void CreateCubePrefab()
    {
        _grassPrefab = GameObject.Create(name: "cube", runtimeOnly: true, visibleInHierarchy: true);
        _grassPrefab.AddComponent<BoxShape>();
        ModelRenderer modelRenderer = _grassPrefab.AddComponent<ModelRenderer>();
        modelRenderer.NeedsToSetupMesh = false;

        _grassPrefab.Awake();

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
        }
    }

    private void CreateWaterPrefab()
    {
        _waterPrefab = GameObject.Create(name: "water", runtimeOnly: true, visibleInHierarchy: true);
        BoxShape boxShape = _waterPrefab.AddComponent<BoxShape>();
        boxShape.Pivot = new Vector3(0.5f, 0f, 0.5f);
        ModelRenderer modelRenderer = _waterPrefab.AddComponent<ModelRenderer>();
        modelRenderer.NeedsToSetupMesh = false;

        _waterPrefab.Awake();

        string modelPath = Path.Combine("3D", "Basic", "plane.obj");
        Asset_Model model = Tofu.AssetLoadManager.Get<Asset_Model>(modelPath);
        modelRenderer.RuntimeMesh = model.GetMesh(0);


        if (_waterMaterial == null)
        {
            _waterMaterial = Tofu.AssetLoadManager.CreateCopyFile<Asset_Material>(modelRenderer.Material,
                folder: Folders.MaterialsInAssets);

            _waterMaterial.MetallicTextureStrength = 0.2f;
            _waterMaterial.Smoothness = 0f;

            _waterMaterial.AlbedoTexture =
                Tofu.AssetLoadManager.Get<RuntimeTexture>(Folders.Get2DAssetPath("water_still.png"));
            _waterMaterial.AlbedoColor = new Color(0, 255, 255, 255);

            _waterMaterial.MaterialType = MaterialType.Unlit;
            _waterMaterial.Tiling = new Vector2(1, 0.05f);
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
        foreach (Transform child in Transform.Children)
        {
            child.GameObject.Destroy();
        }

        Transform.Children = new HashSet<Transform>();
    }

    private void StartTerrainGenerationOnNewThread()
    {
        if (_grassPrefab == null)
        {
            CreateCubePrefab();
        }

        if (_waterPrefab == null)
        {
            CreateWaterPrefab();
        }

        _grassPrefab.SetActive(true);
        _waterPrefab.SetActive(true);

        Tofu.SceneSerializer.SaveClipboardGameObject(_grassPrefab);

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
                GenerateTerrain(capturedThreadIndex, numberOfThreads));
            threads.Add(thread);
        }

        threads.ForEach(t => t.Start());
    }

    private void StartTerrainGenerationOnMainThread()
    {
        if (_grassPrefab == null)
        {
            return;
        }

        Tofu.SceneSerializer.SaveClipboardGameObject(_grassPrefab);

        DestroyTerrain();
        _concurrentBag.Clear();

        GenerateTerrain(0, 1);
    }


    private void GenerateTerrain(int threadIndex, int numberOfThreads)
    {
        Debug.StartTimer($"Thread #{threadIndex} finished");

        int totalBlocks = TerrainSize * TerrainSize;
        int blocksPerThread = totalBlocks / numberOfThreads;
        int startIndex = blocksPerThread * threadIndex;
        int endIndex;
        if (threadIndex == numberOfThreads - 1)
        {
            endIndex = totalBlocks;
        }
        else
        {
            endIndex = blocksPerThread + threadIndex * blocksPerThread;
        }

        for (int i = startIndex; i < endIndex; i++)
        {
            int x = i % TerrainSize;
            int z = i / TerrainSize;


            float positionY = Mathf.Sin(x / 10f) * Mathf.Cos((float)z / 10) * 15;
            bool isWater = positionY < -1;

            if (positionY < -1)
            {
                positionY = 0;
            }

            GameObject go;
            if (isWater)
            {
                go = (GameObject)_waterPrefab.Clone(false);
                go.GetComponent<Renderer>().Material = _waterMaterial;
            }
            else
            {
                go = (GameObject)_grassPrefab.Clone(false);
                go.GetComponent<Renderer>().Material = _grassMaterial;
            }

            go.Name = $"Thread:{threadIndex} go {i}";
            go.RuntimeOnly = true;


            positionY = positionY.TranslateToGrid(2);

            go.Transform.LocalPosition = new Vector3(x * _cubeModelSize, positionY, z * _cubeModelSize);
            
            go.SetActive(true);


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
        Tofu.SceneManager.CurrentScene.AddGameObjectsToScene(_concurrentBag);
        foreach (GameObject go in _concurrentBag)
        {
            // Vector3 oldPosition = Transform.WorldPosition;
            Transform.WorldPosition = Vector3.Zero;
            go.Transform.SetParent(Transform);

            // Transform.WorldPosition = oldPosition;
            // go.SetActive(true);
        }

        Debug.EndAndLogTimer(
            $"TerrainGeneration {TerrainSize}x{TerrainSize} - Total of {TerrainSize * TerrainSize} blocks");

        _grassPrefab.SetActive(false);
        _waterPrefab.SetActive(false);
        
        GameObject.IsStaticSelf = true;
        foreach (Transform child in GameObject.Transform.Children)
        {
            child.GameObject.IsStaticSelf =true;
        }
    }

    // private void LongTask()
    // {
    //     List<GameObject> gameObjects = new List<GameObject>(20000);
    //     for (int i = 0; i < 20000; i++)
    //     {
    //         GameObject go = GameObject.Create(name: i.ToString(), addToScene: false);
    //         gameObjects.Add(go);
    //         Debug.Log(i);
    //     }
    //
    //     lock (Tofu.SceneManager.CurrentScene.GameObjects)
    //     {
    //         Tofu.SceneManager.CurrentScene.AddGameObjectsToScene(gameObjects);
    //     }
    // }
}