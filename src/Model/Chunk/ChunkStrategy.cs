using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.Model.Chunk;

public abstract class ChunkStrategy
{
    public abstract ChunkState getChunkStateOfStrategy();
    
    protected Chunk chunk;
    protected World world;

    public ChunkStrategy(Chunk chunk, World world) {
        this.chunk = chunk;
        this.world = world;
    }
    
    public virtual BlockData getBlockData(Vector3D<int> localPosition) {
        if (localPosition.X < 0 || localPosition.X >= Chunk.CHUNK_SIZE ||
            localPosition.Y < 0 || localPosition.Y >= Chunk.CHUNK_SIZE ||
            localPosition.Z < 0 || localPosition.Z >= Chunk.CHUNK_SIZE) return world.getBlockData(chunk.position + localPosition);
        return chunk.blocks[localPosition.X, localPosition.Y, localPosition.Z];
    }

    public virtual void updateChunkVertex() {
        throw new Exception("try to update Chunk Vertex on a non initialized chunk");
    }
    
    public virtual void draw(GL gl, double deltaTime){}

    public abstract void setBlock(int x, int y, int z, string name);

    public virtual Block getBlock(int x, int y, int z) {
        if (x >= Chunk.CHUNK_SIZE || x < 0 ||
            y >= Chunk.CHUNK_SIZE || y < 0 ||
            z >= Chunk.CHUNK_SIZE || z < 0) return world.getBlock(chunk.position + new Vector3D<int>(x, y, z));
        var blockData = chunk.blocks[x, y, z];
        return Chunk.blockFactory.buildFromBlockData(new Vector3D<int>(x, y, z), blockData);
    }
}