namespace Tofu3D.Rendering.Instancing;

public record InstancedGroupDefinition(
    string GameObjectNameForTestingIdentification,
    RuntimeMesh RuntimeMesh,
    Asset_Material Material,
    bool IsStatic,
    VertexBufferStructureType vertexBufferStructureType);
// int index);