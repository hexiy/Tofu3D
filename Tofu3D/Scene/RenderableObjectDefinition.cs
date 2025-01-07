namespace Tofu3D.Rendering.Instancing;

public record RenderableObjectDefinition(
    string GameObjectNameForTestingIdentification,
    RuntimeMesh RuntimeMesh,
    Asset_Material Material,
    bool IsStatic,
    VertexBufferStructureType vertexBufferStructureType);
// int index);