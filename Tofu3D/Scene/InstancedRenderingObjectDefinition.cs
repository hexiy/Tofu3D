namespace Tofu3D;

public record InstancedRenderingObjectDefinition(Mesh Mesh, Material Material, bool IsStatic, VertexBufferStructureType vertexBufferStructureType);