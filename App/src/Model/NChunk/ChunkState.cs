namespace MinecraftCloneSilk.Model.NChunk;

[Flags]
public enum ChunkState
{
    UNKNOW = 0,
    EMPTY = 1,
    STORAGELOADING = 2,
    STORAGELOADED = 4,
    TERRAINLOADING = 8,
    GENERATEDTERRAIN = 16,
    BLOCKLOADING = 32,
    BLOCKGENERATED = 64,
    LIGHTLOADING = 128,
    LIGHTING = 256,
    DRAWLOADING = 512,
    DRAWABLE = 1024
}

public static class ChunkStateTools
{
    public const ChunkState ALLLOADED = ChunkState.STORAGELOADED | ChunkState.GENERATEDTERRAIN | ChunkState.BLOCKGENERATED | ChunkState.LIGHTING | ChunkState.DRAWABLE;

    public const ChunkState ALLLOADING = ChunkState.STORAGELOADING |
                                         ChunkState.TERRAINLOADING |
                                         ChunkState.BLOCKLOADING |
                                         ChunkState.LIGHTLOADING |
                                         ChunkState.DRAWLOADING;
    
    public static bool IsChunkIsLoading(ChunkState chunkstate) {
        return (chunkstate & ALLLOADING) > 0;
    }
}