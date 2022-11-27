using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Chunk;

public class ChunkTerrainAndStructuresStrategy : ChunkStrategy
{
    public override ChunkState getChunkStateOfStrategy() => ChunkState.GENERATEDTERRAINANDSTRUCTURES;
    public override void setBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = name.GetHashCode();
    }

    public ChunkTerrainAndStructuresStrategy(Chunk chunk, World world) : base(chunk, world) {
    }

    public override async Task init() {
        if (chunk.chunkState != ChunkState.Generatedterrain) {
            chunk.chunkStrategy = new ChunkTerrainGeneratedStrategy(chunk, world);
            await chunk.chunkStrategy.init();
            chunk.chunkStrategy = this;
        }
        await updateNeighboorChunkState(ChunkState.Generatedterrain);
        chunk.chunkState = ChunkState.GENERATEDTERRAINANDSTRUCTURES;
    }

    
}