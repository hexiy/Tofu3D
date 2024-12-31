namespace Tofu3D;

public record InstancedRenderingObjectDefinition(
    string GameObjectNameForTestingIdentification,
    RuntimeMesh RuntimeMesh,
    Asset_Material Material,
    bool IsStatic,
    VertexBufferStructureType vertexBufferStructureType);
// int index);