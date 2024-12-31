using LibNoise;
using LibNoise.Primitive;
using Newtonsoft.Json;

namespace Tofu3D;

public class VertexManipulator : Component, IComponentUpdateable
{
    [XmlIgnore]
    public Action CreateInstanceOfMeshAsset
    {
        get
        {
            return () =>
            {
                _modelRendererInstanced = GetComponent<ModelRendererInstanced>();
                // Asset_Mesh meshAsset =
                // Tofu.AssetLoadManager.Load<Asset_Mesh>(
                // sourcePath: _modelRendererInstanced.RuntimeMesh.MeshAssetPath,
                // loadParameters:new AssetLoadParameters_AssetMesh(){ExistingAsset = null, PathToAsset = _modelRendererInstanced.RuntimeMesh.MeshAssetPath},
                // overwriteAlreadyLoadedAssets: true, isRuntimeCopy: true);
                _assetMesh =
                    Serializer.ReadAssetJSON<Asset_Mesh>(_modelRendererInstanced.RuntimeMesh.MeshAssetPath);
if(_assetMesh.IsRuntimeCopy==false){
                _assetMesh = _assetMesh.CreateRuntimeCopy();
                }

                // _runtimeMesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(_assetMesh.PathToAssetInLibrary,
                //     new AssetLoadParameters_RuntimeMesh()
                //         { ExistingAsset = _runtimeMesh, PathToAsset = _assetMesh.PathToAssetInLibrary });

                _runtimeMesh = Tofu.AssetLoadManager.LoadRuntimeMeshFromAssetMesh<RuntimeMesh>(_assetMesh,
                    new AssetLoadParameters_RuntimeMesh()
                        { ExistingAsset = _runtimeMesh, PathToAsset = _assetMesh.PathToAssetInLibrary });

                _runtimeMesh.PathToAssetInLibrary = _assetMesh.PathToAssetInLibrary;
                // RuntimeMesh 

                // Tofu.InstancedRenderingSystem.UpdateObjectData(_modelRendererInstanced, ref _modelRendererInstanced.InstancingData, remove: true,
                // vertexBufferStructureType: VertexBufferStructureType.Model);
                _modelRendererInstanced.RuntimeMesh = _runtimeMesh;

                // Tofu.AssetLoadManager.Load<RuntimeMesh>(sourcePath: _mesh.PathToAssetInLibrary, overwriteAlreadyLoadedAssets:true, isRuntimeCopy: true);
            };
        }
    }

    [XmlIgnore]
    public Action ModifyMeshButton
    {
        get { return () => ModifyMeshMethod(); }
    }

    private void ModifyMeshMethod()
    {
// do changes
        if (OriginalVertexBufferData == null ||
            OriginalVertexBufferData?.Length != _assetMesh.VertexBufferData.Length)
        {
            OriginalVertexBufferData = new float[_assetMesh.VertexBufferData.Length];
            _assetMesh.VertexBufferData.CopyTo(OriginalVertexBufferData, 0);
        }

        // _perlin.Quality = NoiseQuality.Fast;
        for (int i = 0; i < _assetMesh.VertexBufferData.Length; i++)
        {
            _assetMesh.VertexBufferData[i] = OriginalVertexBufferData[i] +
                                             OriginalVertexBufferData[i] * 0.8f * _perlin.GetValue(
                                                 OriginalVertexBufferData[i], 0.5f,
                                                 Time.EditorElapsedTime);
            // _assetMesh.VertexBufferData[i] = OriginalVertexBufferData[i] + Mathf.Sin(Time.EditorElapsedTime*2)*0.4f;
        }
        // _assetMesh.CompressData();

// save it first
        // Tofu.AssetLoadManager.Save<Asset_Mesh>(_assetMesh.PathToAssetInLibrary, _assetMesh);

        // load
        _runtimeMesh = Tofu.AssetLoadManager.LoadRuntimeMeshFromAssetMesh<RuntimeMesh>(_assetMesh,
            new AssetLoadParameters_RuntimeMesh()
                { ExistingAsset = _runtimeMesh, PathToAsset = _assetMesh.PathToAssetInLibrary });

        // _runtimeMesh = Tofu.AssetLoadManager.Load<RuntimeMesh>(_assetMesh.PathToAssetInLibrary,
        // new AssetLoadParameters_RuntimeMesh()
        // { ExistingAsset = _runtimeMesh, PathToAsset = _assetMesh.PathToAssetInLibrary },
        // overwriteAlreadyLoadedAssets: false,
        // isRuntimeCopy: true);

        _runtimeMesh.PathToAssetInLibrary = _assetMesh.PathToAssetInLibrary;
        // RuntimeMesh 
        _modelRendererInstanced.RuntimeMesh = _runtimeMesh;
        
        Tofu.InstancedRenderingSystem.UpdateObjectData(_modelRendererInstanced, ref _modelRendererInstanced.InstancingData, remove: true,
            vertexBufferStructureType: VertexBufferStructureType.Model);
        
        _modelRendererInstanced.InstancingData = new RendererInstancingData();
        _modelRendererInstanced.InstancingData.InstancedRenderingDefinitionIndex = -1;
        


    }

    private ModelRendererInstanced _modelRendererInstanced;
    private RuntimeMesh _runtimeMesh;
    private Asset_Mesh _assetMesh;
    private ImprovedPerlin _perlin;

    [JsonIgnore]
    public float[] OriginalVertexBufferData;

    public override void Awake()
    {
        base.Awake();
    }

    [ExecuteInEditMode]
    public override void Start()
    {
        base.Start();
    }

    public void Update()
    {
        if (_perlin == null)
        {
            _perlin = new ImprovedPerlin();
        }

        if (_assetMesh == null)
        {
            CreateInstanceOfMeshAsset();
            return;
        }

        ModifyMeshMethod();
    }
}