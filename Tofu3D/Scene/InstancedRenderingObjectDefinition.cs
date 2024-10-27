namespace Tofu3D;

public record InstancedRenderingObjectDefinition(
    RuntimeMesh RuntimeMesh,
    Asset_Material Material,
    bool IsStatic,
    VertexBufferStructureType vertexBufferStructureType);