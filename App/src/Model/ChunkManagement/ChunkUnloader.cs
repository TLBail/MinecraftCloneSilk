using System.Collections.Concurrent;
using System.Diagnostics;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public class ChunkUnloader : IDisposable
{
    
    
    
    public void ChunkUnloaderProcessor() {
        foreach (Chunk chunk in chunksToUnload.GetConsumingEnumerable(cancellationTokenSource.Token)) {
            chunkStorage.SaveChunk(chunk);
            chunkPool.ReturnChunk(chunk);
        }
    }
    
    private IChunkStorage chunkStorage;
    private ChunkPool chunkPool;
    
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly BlockingCollection<Chunk> chunksToUnload = new BlockingCollection<Chunk>();
    private readonly Task chunkUnloaderTask;



    public ChunkUnloader(IChunkStorage chunkStorage, ChunkPool chunkPool) {
        this.chunkStorage = chunkStorage;
        this.chunkPool = chunkPool;
        chunkUnloaderTask = new Task(ChunkUnloaderProcessor);
        chunkUnloaderTask.Start();
    }
    
    
    public void AddChunkToUnload(Chunk chunk) {
        if (chunk.chunkStateInStorage > chunk.chunkState) {
            throw new Exception("try to unload a chunk with a lower state than the one in storage");
        }
        chunk.blockModified = false; // Todo voir si on peut pas faire mieux
        chunksToUnload.Add(chunk);
    }
    
    
    
    public void Update() {
    }

    public void Dispose() {
        chunksToUnload.CompleteAdding();
        cancellationTokenSource.Cancel();
        chunksToUnload.Dispose();
        chunkUnloaderTask.Dispose();
    }
}