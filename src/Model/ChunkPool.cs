using System.Collections.Concurrent;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public class ChunkPool : IDisposable
{

    private readonly ConcurrentBag<Chunk> chunkPool = new ConcurrentBag<Chunk>();

    public IChunkManager chunkManager { get; init; }
    public WorldGenerator worldGenerator { get; init; }
    
    public ChunkPool(IChunkManager chunkManager, WorldGenerator worldGenerator) {
        this.chunkManager = chunkManager;
        this.worldGenerator = worldGenerator;
    }

    public int count() => chunkPool.Count;
    public Chunk get(Vector3D<int> position) {
      Chunk chunk = chunkPool.TryTake(out Chunk result) ? result : buildChunk(position);
      chunk.reset(position, chunkManager, worldGenerator);
      return chunk;
    }

    public void returnChunk(Chunk chunk) {
        if (chunk.blockModified) {
            throw new GameException("try to return a modified chunk");
        }
        chunkPool.Add(chunk);  
    } 
    
    private Chunk buildChunk(Vector3D<int> position) {
        return new Chunk(position, chunkManager, worldGenerator);
    }


    public void Dispose() {
        foreach (Chunk chunk in chunkPool) {
            chunk.Dispose();
        }
    }
}