using System.Collections.Concurrent;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Lighting;

public class ChunkLightManager : IChunkLightManager, IDisposable 
{
    private class ILightTask(SemaphoreSlim semaphore)
    {
        public SemaphoreSlim semaphore = semaphore;
    }
    
    private class FullLightTask  : ILightTask
    {
        public FullLightTask(SemaphoreSlim semaphore, Chunk chunk) : base(semaphore)
        {
            this.chunk = chunk;
        }
        public Chunk chunk;
    }

    private class OnBlockSetLightTask : ILightTask
    {
        public OnBlockSetLightTask(SemaphoreSlim semaphore, Chunk chunk, Vector3D<int> position, BlockData oldBlockData,
            BlockData newBlockData) :base(semaphore) {
            this.chunk = chunk;
            this.position = position;
            this.oldBlockData = oldBlockData;
            this.newBlockData = newBlockData;
        }
        public Chunk chunk;
        public Vector3D<int> position;
        public BlockData oldBlockData;
        public BlockData newBlockData;
    }

    private void ChunkLightProcessor() {
        foreach(ILightTask task in chunkLightingTask.GetConsumingEnumerable()) {
            switch (task) {
                case FullLightTask fullLightTask:
                    LightCalculator.LightChunk(fullLightTask.chunk);
                    break;
                case OnBlockSetLightTask onBlockSetLightTask:
                    LightCalculator.OnBlockSet(onBlockSetLightTask.chunk, onBlockSetLightTask.position, onBlockSetLightTask.oldBlockData, onBlockSetLightTask.newBlockData);
                    break;
            }
            task.semaphore.Release();
        }
    }
    
    private readonly BlockingCollection<ILightTask> chunkLightingTask = new BlockingCollection<ILightTask>();
    private readonly Task chunkLightProcessorSystemTask;
    
    public ChunkLightManager() {
        chunkLightProcessorSystemTask = new Task(ChunkLightProcessor);
        chunkLightProcessorSystemTask.Start();
    }
    
    public SemaphoreSlim FullLightChunk(Chunk chunk) {
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(0);
        chunkLightingTask.Add(new FullLightTask(semaphoreSlim,chunk));
        return semaphoreSlim;
    }
    
    public SemaphoreSlim OnBlockSet(Chunk chunk, Vector3D<int> position, BlockData oldBlockData, BlockData newBlockData) {
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(0);
        chunkLightingTask.Add(new OnBlockSetLightTask(semaphoreSlim,chunk, position, oldBlockData, newBlockData));
        return semaphoreSlim;
    }


    public void Dispose() {
        chunkLightingTask.CompleteAdding();
        chunkLightProcessorSystemTask.Wait();
        chunkLightingTask.Dispose();
        chunkLightProcessorSystemTask.Dispose();
    }
}