namespace Tofu3D;

public record InstancedRenderingObjectDefinition(
    RuntimeMesh AssetMesh,
    Asset_Material Material,
    bool IsStatic,
    VertexBufferStructureType vertexBufferStructureType);