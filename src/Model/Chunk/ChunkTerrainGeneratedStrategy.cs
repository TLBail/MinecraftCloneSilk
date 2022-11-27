using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Chunk;

public class ChunkTerrainGeneratedStrategy : ChunkStrategy
{
    private WorldGeneration worldGeneration;
    
    public ChunkTerrainGeneratedStrategy(Chunk chunk, World world) : base(chunk, world) {
        this.worldGeneration = world.worldGeneration;

        if (chunk.chunkState != ChunkState.EMPTY) {
            chunk.chunkStrategy = new ChunkEmptyStrategy(chunk, world);
        }
        generateTerrain();
        chunk.chunkState = ChunkState.Generatedterrain;
    }

    public override ChunkState getChunkStateOfStrategy() => ChunkState.Generatedterrain;
    

    public override BlockData getBlockData(Vector3D<int> localPosition) {
        chunk.setWantedChunkState(ChunkState.GENERATEDTERRAINANDSTRUCTURES);
        return chunk.getBlockData(localPosition);
    }

    public override void setBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = name.GetHashCode();
    }

    private void generateTerrain()
    {
        worldGeneration.generateTerrain(chunk.position, chunk.blocks);
    }
}