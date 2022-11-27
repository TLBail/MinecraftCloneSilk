using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Chunk;

public class ChunkTerrainGeneratedStrategy : ChunkStrategy
{
    private WorldGeneration worldGeneration;
    
    public ChunkTerrainGeneratedStrategy(Chunk chunk, World world) : base(chunk, world) {
        this.worldGeneration = world.worldGeneration;
    }

    public override async Task init() {
        if (chunk.chunkState != ChunkState.EMPTY) {
            chunk.chunkStrategy = new ChunkEmptyStrategy(chunk, world); 
            await chunk.chunkStrategy.init();
            chunk.chunkStrategy = this;
        }
        generateTerrain();
        chunk.chunkState = ChunkState.Generatedterrain;
    }

    public override ChunkState getChunkStateOfStrategy() => ChunkState.Generatedterrain;
    

    public override async Task<BlockData> getBlockData(Vector3D<int> localPosition) {
        await chunk.setWantedChunkState(ChunkState.GENERATEDTERRAINANDSTRUCTURES);
        return await chunk.getBlockData(localPosition);
    }

    public override void setBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = name.GetHashCode();
    }

    private void generateTerrain()
    {
        worldGeneration.generateTerrain(chunk.position, chunk.blocks);
    }
}