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
    public enum ChunkLoaderMode
    {
        SYNC,
        ASYNC
    }
    private interface ChunkTaskLoader
    {
        public bool LoadChunkTask(ChunkLoadingTask chunkLoadingTask);
    }

    public class ChunkTaskLoaderSync : ChunkTaskLoader
    {
        public ChunkLoader chunkLoader;
        public ChunkTaskLoaderSync(ChunkLoader chunkLoader) { this.chunkLoader = chunkLoader;}
        
        public bool LoadChunkTask(ChunkLoadingTask chunkLoadingTask) {
            chunkLoadingTask.chunk.LoadChunkState();
            chunkLoader.chunksToFinish.Add(chunkLoadingTask);
            return true;
        }
    }
    public class ChunkTaskLoaderAsync : ChunkTaskLoader
    {
        private ChunkLoader chunkLoader;

        public ChunkTaskLoaderAsync(ChunkLoader chunkLoader) { this.chunkLoader = chunkLoader;}
        public void ThreadChunkLoading(Object chunkLoadingTask) {
            ((ChunkLoadingTask)chunkLoadingTask).chunk.LoadChunkState();
            chunkLoader.chunksToFinish.Add((ChunkLoadingTask)chunkLoadingTask);
        }
        public bool LoadChunkTask(ChunkLoadingTask chunkLoadingTask) {
            return ThreadPool.QueueUserWorkItem(ThreadChunkLoading, chunkLoadingTask);
        }
    }



    private ChunkTaskLoader chunkTaskLoader; 
    public LinkedList<ChunkLoadingTask> chunkTasks = new();
    public ConcurrentBag<ChunkLoadingTask> chunksToFinish = new();
    private Stopwatch stopwatch = new Stopwatch();
    public ChunkLoader(ChunkLoaderMode mode) {
        switch (mode) {
            case ChunkLoaderMode.SYNC:
                this.chunkTaskLoader = new ChunkTaskLoaderSync(this);
                break;
            case ChunkLoaderMode.ASYNC:
                this.chunkTaskLoader = new ChunkTaskLoaderAsync(this);
                break;
        }
    }

    public void Update() {
        stopwatch.Restart();
        while(chunkTasks.Count > 0 && stopwatch.ElapsedMilliseconds < 10) {
            UpdateJob();
        }
        stopwatch.Restart();
        while(chunksToFinish.Count > 0 && stopwatch.ElapsedMilliseconds < 10) {
            FinishJob();
        }
        stopwatch.Stop();
    }
    
    public void LoadAllChunks() {
        while(chunkTasks.Count > 0) {
            UpdateJob();
            while(chunksToFinish.Count > 0) {
                FinishJob();
            }
        }
    }

    private void UpdateJob() {
        ChunkLoadingTask chunkTask = chunkTasks.First!.Value;
        chunkTasks.RemoveFirst();


        if (ChunkStateTools.IsChunkIsLoading(chunkTask.chunk.chunkState)) {
            chunkTasks.AddLast(chunkTask);
            return;
        }
            
        if(chunkTask.chunk.chunkState >= chunkTask.wantedChunkState) {
            chunkTask.chunk.RemoveRequiredByChunkLoader();
            if (chunkTask.parent is not null) {
                ChunkWaitingTask parent = chunkTask.parent;
                Interlocked.Decrement(ref parent.counter);
                
                if(parent.counter == 0) {
                    chunkTasks.AddFirst(parent.chunkLoadingTask);
                }
            }
            return;
        }

        bool canLoad = chunkTask.chunk.TryToSetChunkState(this, chunkTask); 
        if(!canLoad)return;
                
        chunkTask.chunk.SetChunkState(chunkTask.wantedChunkState);
        chunkTask.chunk.InitChunkState();


        bool added = chunkTaskLoader.LoadChunkTask(chunkTask);
        if (!added) throw new Exception("failed to add chunk to load threadpool");
    }


    private void FinishJob() {
        if(!chunksToFinish.TryTake(out ChunkLoadingTask? chunkTask)) return;
        chunkTask.chunk.FinishChunkState();
        chunkTask.chunk.RemoveRequiredByChunkLoader();
            
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
        chunkLoadingTask.chunk.AddRequiredByChunkLoader();
        return true;
    }

    public void AddChunkToQueue(Chunk chunk, ChunkState chunkState = ChunkState.DRAWABLE) {
        chunk.AddRequiredByChunkLoader();
        chunkTasks.AddLast(new ChunkLoadingTask(chunk, chunkState, null));
    }
}