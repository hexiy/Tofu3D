using System.Linq;
using System.Runtime.CompilerServices;

namespace Tofu3D;

public class VertexBuffer
{
    private int ElementsPerVertex { get; init; }

    // public int Vbo { get; private init; }
    private int SizeOfElementInBytes { get; init; }
    private VertexAttribPointerType VertexAttribPointerType { get; init; }

    private VertexBuffer(int elementsPerVertex /*, int vbo*/, int sizeOfElementInBytes,
        VertexAttribPointerType vertexAttribPointerType)
    {
        SizeOfElementInBytes = sizeOfElementInBytes;
        ElementsPerVertex = elementsPerVertex;
        VertexAttribPointerType = vertexAttribPointerType;
    }

    public static VertexBuffer Create<T>(BufferTarget bufferTarget, T[] vertexData, int elementsPerVertex)
    {
        int vbo = GL.GenBuffer();
        GL.BindBuffer(bufferTarget, vbo);
        int sizeOfElementInBytes = Unsafe.SizeOf<T>();
        if (sizeOfElementInBytes == 0)
            throw new ArgumentNullException(
                "Define VertexBuffer data value type(float,uint) as the generic parameter Create<T>");

        VertexAttribPointerType vertexAttribPointerType = VertexAttribPointerType.Float;

        if (bufferTarget == BufferTarget.ArrayBuffer)
        {
            float[] vertexDataFloats = vertexData.Cast<float>().ToArray();
            vertexAttribPointerType = VertexAttribPointerType.Float;
            GL.BufferData(bufferTarget, sizeOfElementInBytes * vertexDataFloats.Length, vertexDataFloats,
                BufferUsageHint.StaticDraw);
        }

        if (bufferTarget == BufferTarget.ElementArrayBuffer)
        {
            uint[] vertexDataInts = vertexData.Cast<uint>().ToArray();
            vertexAttribPointerType = VertexAttribPointerType.UnsignedInt;

            GL.BufferData(bufferTarget, sizeOfElementInBytes * vertexDataInts.Length, vertexDataInts,
                BufferUsageHint.StaticDraw);
        }

        VertexBuffer vertexBuffer = new(elementsPerVertex, sizeOfElementInBytes, vertexAttribPointerType);
        return vertexBuffer;
    }

    public void EnableAttribs(bool sequential = true, params int[] countsOfElements)
    {
        int nextAttribIndex = 0;
        int currentAttribOffset = 0;
        foreach (int countOfElements in countsOfElements)
        {
            GL.VertexAttribPointer(nextAttribIndex, countOfElements, VertexAttribPointerType, false,
                ElementsPerVertex * SizeOfElementInBytes,
                (IntPtr)(sequential ? currentAttribOffset * SizeOfElementInBytes : 0));

            GL.EnableVertexAttribArray(nextAttribIndex);
            currentAttribOffset += countOfElements;
            nextAttribIndex++;
        }
    }
}