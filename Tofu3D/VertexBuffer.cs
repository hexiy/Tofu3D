using System.Runtime.CompilerServices;

namespace Tofu3D;

public class VertexBuffer
{
    private VertexBuffer(int elementsPerVertex /*, int vbo*/, int sizeOfElementInBytes,
        VertexAttribPointerType vertexAttribPointerType)
    {
        SizeOfElementInBytes = sizeOfElementInBytes;
        ElementsPerVertex = elementsPerVertex;
        VertexAttribPointerType = vertexAttribPointerType;
    }

    private int ElementsPerVertex { get; }

    // public int Vbo { get; private init; }
    private int SizeOfElementInBytes { get; }
    private VertexAttribPointerType VertexAttribPointerType { get; }

    public static VertexBuffer Create<T>(BufferTarget bufferTarget, T[] vertexData, int elementsPerVertex,
        bool isDynamic = false) where T : unmanaged // Ensure T is a value type (no nullable or reference types)
    {
        int vbo = GL.GenBuffer();
        GL.BindBuffer(bufferTarget, vbo);
        var sizeOfElementInBytes = Unsafe.SizeOf<T>();
        if (sizeOfElementInBytes == 0)
        {
            throw new ArgumentNullException(
                "Define VertexBuffer data value type(float,uint) as the generic parameter Create<T>");
        }

        var vertexAttribPointerType = VertexAttribPointerType.Float;

        if (bufferTarget == BufferTarget.ArrayBuffer)
        {
            vertexAttribPointerType = VertexAttribPointerType.Float;
        }

        if (bufferTarget == BufferTarget.ElementArrayBuffer)
        {
            vertexAttribPointerType = VertexAttribPointerType.UnsignedInt;
        }

        GL.BufferData(bufferTarget, sizeOfElementInBytes * vertexData.Length, vertexData,
            isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);

        VertexBuffer vertexBuffer = new(elementsPerVertex, sizeOfElementInBytes, vertexAttribPointerType);
        return vertexBuffer;
    }

    public void EnableAttribs(bool sequential = true, params int[] countsOfElements)
    {
        var nextAttribIndex = 0;
        var currentAttribOffset = 0;
        foreach (var countOfElements in countsOfElements)
        {
            GL.EnableVertexAttribArray(nextAttribIndex);

            GL.VertexAttribPointer(index: nextAttribIndex, size: countOfElements, type: VertexAttribPointerType, false,
                stride: ElementsPerVertex * SizeOfElementInBytes,
                (IntPtr)(sequential ? (currentAttribOffset) * SizeOfElementInBytes : 0));

            currentAttribOffset += countOfElements;
            nextAttribIndex++;
        }
    }
}