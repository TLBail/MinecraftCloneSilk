using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public class ChunkManager : IChunkManager, IDisposable
{
    
    private Dictionary<Vector3D<int>, Chunk> chunks;
    private object chunksLock = new object();
    public WorldGenerator worldGenerator { get;  set; }


    public struct ChunkLoadingTask
    {
        public Chunk chunk { get; set; }
        public ChunkState chunkState { get; set; }

        public ChunkLoadingTask(Chunk chunk, ChunkState chunkState) {
            this.chunk = chunk;
            this.chunkState = chunkState;
        }
    }
    
    private List<ChunkLoadingTask> chunksToLoad;
    private object chunksToLoadLock = new object();
    private Thread chunkLoaderThread;
    private bool runThread = true;
    private SemaphoreSlim semaphore;

    private List<Chunk> chunksToUpdate = new List<Chunk>();
    private List<Chunk> chunksToDraw = new List<Chunk>();
    public List<Chunk> getChunksToDraw() => chunksToDraw;

    private List<Chunk> chunksToUnload = new List<Chunk>();
    private object chunksToUnloadLock = new object();
    
    private ChunkPool chunkPool;

    public ChunkManager(int RADIUS, WorldGenerator worldGenerator) {
        chunks = new  Dictionary<Vector3D<int>, Chunk>(RADIUS * RADIUS * RADIUS);
        this.worldGenerator = worldGenerator;
        chunksToLoad = new List<ChunkLoadingTask>();
        semaphore = new SemaphoreSlim(0);
        chunkPool = new ChunkPool(this, worldGenerator);
        chunkLoaderThread = new Thread(chunkLoaderThreadRuntime);
        chunkLoaderThread.Start();
    }

    public void chunkLoaderThreadRuntime() {
        while (runThread) {
            semaphore.Wait();
            List<ChunkLoadingTask> chunksToLoadCopy;
            lock (chunksToLoadLock) {
                chunksToLoadCopy = new List<ChunkLoadingTask>(chunksToLoad);
                chunksToLoad.Clear();
            }
            foreach (ChunkLoadingTask task in chunksToLoadCopy) {
                task.chunk.setMinimumWantedChunkState(task.chunkState);
            }

            List<Chunk> chunksToUnloadCopy;
            lock (chunksToUnloadLock) {
                chunksToUnloadCopy = new List<Chunk>(chunksToUnload);
                chunksToUnload.Clear();
            }

            foreach (Chunk chunk in chunksToUnloadCopy) {
                chunkPool.returnChunk(chunk);
            }
            
        }
    }


    public void update(double deltatime) {
        
        foreach (Chunk chunk in chunksToUpdate) {
            chunk.Update(deltatime);
        }
    }

    public int count() => chunks.Count;
    public ImmutableDictionary<Vector3D<int>,Chunk> getImmutableDictionary() => chunks.ToImmutableDictionary();
    public bool ContainsKey(Vector3D<int> position) => chunks.ContainsKey(position);
    
    public void clear() {
        lock (chunksLock) chunks.Clear();
    }
    
    public Chunk getChunk(Vector3D<int> position) {
        lock (chunksLock) {
            if (chunks.ContainsKey(position)) {
                return chunks[position];
            }
            Chunk chunk = new Chunk(position, this, worldGenerator);
            chunks.Add(position, chunk);
            return chunk;
        }
    }

    public void addChunkToDraw(Chunk chunk) {
        chunksToDraw.Add(chunk);
    }

    public void addChunkToUpdate(Chunk chunk) {
        chunksToUpdate.Add(chunk);
    }

    public void updateRelevantChunks(List<Vector3D<int>> chunkRelevant) {
        lock (chunksLock) {
            foreach (Vector3D<int> position in chunkRelevant) {
                if (!chunks.ContainsKey(position)) {
                    chunks.Add(position, new Chunk(position,this, worldGenerator));
                }
            }
            IEnumerable<Vector3D<int>> chunksToDeletePosition = chunks.Keys.Except(chunkRelevant);
            foreach (var chunkToDeletePosition in chunksToDeletePosition) removeChunk(chunks[chunkToDeletePosition]);

            foreach (Vector3D<int> position  in chunkRelevant) {
                Chunk chunk = chunks[position];
                if (chunk.chunkState < ChunkState.DRAWABLE) { 
                    chunksToLoad.Add(new ChunkLoadingTask(chunk, ChunkState.DRAWABLE));
                }
            }   
        }
    }

    public void addChunkToLoad(Vector3D<int> position) {
        lock (chunksToLoadLock) {
            ChunkLoadingTask chunkLoadingTask = new ChunkLoadingTask(chunkPool.get(position), ChunkState.DRAWABLE); 
            chunksToLoad.Add(chunkLoadingTask);
            chunks.Add(chunkLoadingTask.chunk.position, chunkLoadingTask.chunk);
            chunksToUpdate.Add(chunkLoadingTask.chunk);
            if (semaphore.CurrentCount <= 0) semaphore.Release();
        }
    }

    public bool tryToUnloadChunk(Vector3D<int> position) {
        Chunk chunkToUnload = null;
        lock (chunksToUnloadLock) {
            if (chunks.ContainsKey(position)) {
                chunkToUnload = chunks[position];
            }   
        }
        if(chunkToUnload == null) return false;
        Console.WriteLine("remove " + position);
        ChunkState minimumChunkState = getMinimumChunkStateOfChunk(position);
        Console.WriteLine("minimum chunk state " + minimumChunkState.ToString());
        if (minimumChunkState == ChunkState.EMPTY) {
            chunks.Remove(position);
            chunkToUnload.Dispose();
            chunksToUnload.Add(chunkToUnload);
            chunksToDraw.Remove(chunkToUnload);
            chunksToUpdate.Remove(chunkToUnload);
            if (semaphore.CurrentCount <= 0) semaphore.Release();
            Console.WriteLine("chunk deleted");
            return true;
        }
        return false;
    }


    private ChunkState getMinimumChunkStateOfChunk(Vector3D<int> position) {
        ChunkState chunkState = ChunkState.EMPTY;
        Chunk chunk;
        if (chunks.TryGetValue(position + new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0), out chunk) && chunk.getMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.getMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, 0), out chunk) && chunk.getMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.getMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(0, (int)Chunk.CHUNK_SIZE, 0), out chunk) && chunk.getMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.getMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(0, -(int)Chunk.CHUNK_SIZE, 0), out chunk) && chunk.getMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.getMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(0, 0, (int)Chunk.CHUNK_SIZE), out chunk) && chunk.getMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.getMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(0, 0, -(int)Chunk.CHUNK_SIZE), out chunk) && chunk.getMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.getMinimumChunkStateOfNeighbors();
        
        return chunkState;
    }
    
    private void removeChunk(Chunk chunk) {
        chunk.Dispose();
        chunks.Remove(chunk.position);
    }

    public void Dispose() {
        runThread = false;
        chunksToLoad.Clear();
        semaphore.Release();
        chunkLoaderThread.Join();
        chunkPool.Dispose();
    }

}