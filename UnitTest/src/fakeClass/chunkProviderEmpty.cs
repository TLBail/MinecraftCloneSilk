using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace UnitTest.fakeClass;

public class ChunkManagerEmpty : IChunkManager
{
    public Dictionary<Vector3D<int>, Chunk> chunks = new Dictionary<Vector3D<int>, Chunk>();

    public WorldGenerator worldGenerator;

    public ChunkManagerEmpty(WorldGenerator worldGenerator) {
        this.worldGenerator = worldGenerator;
    }

    public Chunk getChunk(Vector3D<int> position) {
        if (chunks.ContainsKey(position)) {
            return chunks[position];
        }    
        chunks.Add(position, new Chunk(position, this, worldGenerator, null));
        return chunks[position];
    }

    public void addChunkToDraw(Chunk chunk) {
    }

    public void addChunkToUpdate(Chunk chunk) {
    }

    public void removeChunkToUpdate(Chunk chunk) {
    }

    public void removeChunkToDraw(Chunk chunk) {
    }
}