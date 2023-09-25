using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;

namespace UnitTest.fakeClass;

public class ChunkManagerEmpty : IChunkManager
{
    public Dictionary<Vector3D<int>, Chunk> chunks = new Dictionary<Vector3D<int>, Chunk>();

    public IWorldGenerator worldGenerator;
    public IChunkStorage chunkStorage;

    public ChunkManagerEmpty(IWorldGenerator worldGenerator, IChunkStorage chunkStorage) {
        this.worldGenerator = worldGenerator;
        this.chunkStorage = chunkStorage;
    }

    public Chunk GetChunk(Vector3D<int> position) {
        if (chunks.ContainsKey(position)) {
            return chunks[position];
        }    
        chunks.Add(position, new Chunk(position, this, worldGenerator, chunkStorage));
        return chunks[position];
    }
    
    public bool ContainChunk(Vector3D<int> position) {
        return chunks.ContainsKey(position);
    }
    

    public void RemoveChunk(Vector3D<int> position) {
        chunks.Remove(position);
    }

    public void addChunkToDraw(Chunk chunk) {
    }

    public void AddChunkToUpdate(Chunk chunk) {
    }

    public void RemoveChunkToUpdate(Chunk chunk) {
    }

    public void removeChunkToDraw(Chunk chunk) {
    }
}