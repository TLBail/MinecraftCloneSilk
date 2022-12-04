using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Chunk;

public class ChunkEmptyStrategy : ChunkStrategy
{
    public ChunkEmptyStrategy(Chunk chunk) : base(chunk) {
    }

    public override ChunkState getChunkStateOfStrategy() => ChunkState.EMPTY;
    

    public override BlockData getBlockData(Vector3D<int> localPosition) {
        throw new Exception("try to access to block data but the chunk is empty");
    }

    public override void setBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = name.GetHashCode();
    }

    public override Task<Block> getBlock(int x, int y, int z) {
        throw new Exception("try to get Block of a empty chunk");
    }
}