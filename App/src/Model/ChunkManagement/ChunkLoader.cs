using System.Collections.Concurrent;
using System.Diagnostics;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public class ChunkWaitingTask
{
    public ChunkLoadingTask chunkLoadingTask { get; set; }
    public int counter;
    public ChunkWaitingTask(ChunkLoadingTask chunkLoadingTask, int counter = 0) {
        this.chunkLoadingTask = chunkLoadingTask;
        this.counter = counter;
    }
}


public class ChunkLoader
{
    public void ThreadChunkLoading(Object chunk) {
        ((ChunkLoadingTask)chunk).chunk.LoadChunkState();
        chunksToFinish.Add((ChunkLoadingTask)chunk);
    }
    
    
    
    public LinkedList<ChunkLoadingTask> chunkTasks = new();
    public ConcurrentBag<ChunkLoadingTask> chunksToFinish = new();

    public ChunkLoader() {
    }
    

    public void Update() {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        while(chunkTasks.Count > 0 && stopwatch.ElapsedMilliseconds < 10) {
            UpdateJob();
        }
        stopwatch.Restart();
        while(chunksToFinish.Count > 0 && stopwatch.ElapsedMilliseconds < 10) {
            FinishJob();
        }
    }
    
    public void LoadAllChunks() {
        while(chunkTasks.Count > 0) {
            UpdateJob();
        }
    }

    private void UpdateJob() {
            ChunkLoadingTask chunkTask = chunkTasks.First!.Value;
            chunkTasks.RemoveFirst();


            if (chunkTask.chunk.chunkState == ChunkState.STORAGELOADING || 
                chunkTask.chunk.chunkState == ChunkState.TERRAINLOADING || 
                chunkTask.chunk.chunkState == ChunkState.BLOCKLOADING ||
                chunkTask.chunk.chunkState == ChunkState.LIGHTLOADING ||
                chunkTask.chunk.chunkState == ChunkState.DRAWLOADING) {
                chunkTasks.AddLast(chunkTask);
                return;
            }
            
            if(chunkTask.chunk.chunkState >= chunkTask.wantedChunkState) {
                chunkTask.chunk.removeRequiredByChunkLoader();
                if (chunkTask.parent is not null) {
                    ChunkWaitingTask parent = chunkTask.parent;
                    Interlocked.Decrement(ref parent.counter);
                
                    if(parent.counter == 0) {
                        chunkTasks.AddFirst(parent.chunkLoadingTask);
                    }
                }
                return;
            }
            
            ChunkWaitingTask? chunkWaitingTask =
                chunkTask.chunk.TryToSetChunkState(this, chunkTask);
            if(chunkWaitingTask is not null) {
                return ;
            }
                
            chunkTask.chunk.SetChunkState(chunkTask.wantedChunkState);
            chunkTask.chunk.InitChunkState();


            ThreadPool.QueueUserWorkItem(ThreadChunkLoading, chunkTask);
    }


    private void FinishJob() {
        if(!chunksToFinish.TryTake(out ChunkLoadingTask? chunkTask)) return;
        chunkTask.chunk.FinishChunkState();
        chunkTask.chunk.removeRequiredByChunkLoader();
            
        if (chunkTask.parent is not null) {
            ChunkWaitingTask parent = chunkTask.parent;
            Interlocked.Decrement(ref parent.counter);
                
            if(parent.counter == 0) {
                chunkTasks.AddFirst(parent.chunkLoadingTask);
            }
        }
    }
    
    public bool NewJob(ChunkLoadingTask chunkLoadingTask) {
        if (chunkLoadingTask.chunk.chunkState >= chunkLoadingTask.wantedChunkState){
            return false;
        }
        chunkTasks.AddFirst(chunkLoadingTask);
        chunkLoadingTask.chunk.addRequiredByChunkLoader();
        return true;
    }

    public void AddChunkToQueue(Chunk chunk, ChunkState chunkState = ChunkState.DRAWABLE) {
        chunk.addRequiredByChunkLoader();
        chunkTasks.AddLast(new ChunkLoadingTask(chunk, chunkState, null));
    }
}