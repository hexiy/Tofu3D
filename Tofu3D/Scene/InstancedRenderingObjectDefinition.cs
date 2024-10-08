namespace Tofu3D;

public record InstancedRenderingObjectDefinition(
    Asset_Mesh AssetMesh,
    Asset_Material Material,
    bool IsStatic,
    VertexBufferStructureType vertexBufferStructureType);