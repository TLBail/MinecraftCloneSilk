namespace MinecraftCloneSilk.Model.NChunk;

public enum ChunkState
{
    UNKNOW = 0, // used for chunkStateInStorage when we don't have check the state in the storage
    EMPTY = 1, // == chunkstate = DEFAULT_CHUNKSTATE  and used for chunkStateInStorage when the chunk is not in the storage
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

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
public static class ChunkStateTools
{
    public const ChunkState ALLLOADED = ChunkState.STORAGELOADED | ChunkState.GENERATEDTERRAIN | ChunkState.BLOCKGENERATED | ChunkState.LIGHTING | ChunkState.DRAWABLE;

    public const ChunkState ALLLOADING = ChunkState.STORAGELOADING |
                                         ChunkState.TERRAINLOADING |
                                         ChunkState.BLOCKLOADING |
                                         ChunkState.LIGHTLOADING |
                                         ChunkState.DRAWLOADING;
    
    public static bool IsChunkIsLoading(ChunkState chunkstate) {
        return (chunkstate & ALLLOADING) != 0;
    }
}