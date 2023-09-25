namespace MinecraftCloneSilk.Model.NChunk;

public enum ChunkState
{
    UNKNOW = -1,
    EMPTY = 0,
    STORAGELOADING = 1,
    STORAGELOADED = 2,
    TERRAINLOADING = 3,
    GENERATEDTERRAIN = 4,
    BLOCKLOADING = 5,
    BLOCKGENERATED = 6,
    LIGHTLOADING = 7,
    LIGHTING = 8,
    DRAWLOADING = 9,
    DRAWABLE = 10,
}