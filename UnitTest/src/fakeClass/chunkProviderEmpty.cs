using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using Silk.NET.Maths;

namespace UnitTest.fakeClass;

public class ChunkManagerEmpty : IChunkManager
{
    public Dictionary<Vector3D<int>, Chunk> chunks = new Dictionary<Vector3D<int>, Chunk>();

    public WorldGenerator worldGenerator;
    public ChunkStorage chunkStorage;

    public ChunkManagerEmpty(WorldGenerator worldGenerator, ChunkStorage chunkStorage) {
        this.worldGenerator = worldGenerator;
        this.chunkStorage = chunkStorage;
    }

    public Chunk getChunk(Vector3D<int> position) {
        if (chunks.ContainsKey(position)) {
            return chunks[position];
        }    
        chunks.Add(position, new Chunk(position, this, worldGenerator, chunkStorage));
        return chunks[position];
    }

    public void removeChunk(Vector3D<int> position) {
        chunks.Remove(position);
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