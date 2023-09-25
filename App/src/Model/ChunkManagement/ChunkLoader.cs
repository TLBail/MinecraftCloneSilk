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
    public LinkedList<ChunkLoadingTask> chunkTasks = new();
    
    public ChunkLoader() {
    }
    

    public void Update() {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        while(chunkTasks.Count > 0 && stopwatch.ElapsedMilliseconds < 10) {
            UpdateJob();
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
            
            if(chunkTask.chunk.chunkState >= chunkTask.wantedChunkState) {
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
                
            chunkTask.chunk.LoadChunkState();
                
            chunkTask.chunk.FinishChunkState();
            
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
        return true;
    }

    public void AddChunkToQueue(Chunk chunk, ChunkState chunkState = ChunkState.DRAWABLE) {
        chunkTasks.AddLast(new ChunkLoadingTask(chunk, chunkState, null));
    }
}