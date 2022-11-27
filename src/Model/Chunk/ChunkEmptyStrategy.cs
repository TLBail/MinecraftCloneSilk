using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Chunk;

public class ChunkEmptyStrategy : ChunkStrategy
{
    public ChunkEmptyStrategy(Chunk chunk, World world) : base(chunk, world) {
    }

    public override ChunkState getChunkStateOfStrategy() => ChunkState.EMPTY;
    

    public override async Task<BlockData> getBlockData(Vector3D<int> localPosition) {
        await chunk.setWantedChunkState(ChunkState.GENERATEDTERRAINANDSTRUCTURES);
        return await chunk.getBlockData(localPosition);
    }

    public override void setBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = name.GetHashCode();
    }

    public override Task<Block> getBlock(int x, int y, int z) {
        throw new Exception("try to get Block of a empty chunk");
    }
}