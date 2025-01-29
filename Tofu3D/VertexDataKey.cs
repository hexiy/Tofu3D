public struct VertexDataKey
{
    public Vector3 Position;  // Position of the vertex
    public Vector2 UV;        // UV coordinates of the vertex
    public Vector3 Normal;    // Normal vector of the vertex

    public override bool Equals(object obj)
    {
        if (!(obj is VertexDataKey))
            return false;

        VertexDataKey other = (VertexDataKey)obj;

        // Compare Position, UV, and Normal for equality
        return Position.Equals(other.Position) &&
               UV.Equals(other.UV) &&
               Normal.Equals(other.Normal);
    }

    public override int GetHashCode()
    {
        // Combine hash codes of Position, UV, and Normal fields
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Position.GetHashCode();
            hash = hash * 31 + UV.GetHashCode();
            hash = hash * 31 + Normal.GetHashCode();
            return hash;
        }
    }
}