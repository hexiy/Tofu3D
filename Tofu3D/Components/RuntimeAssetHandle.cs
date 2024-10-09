using System;
public struct RuntimeAssetHandle
{
    public required int Id { get; set; }
    public required Type AssetType { get; set; }
}