using System.Collections.Concurrent;
using System.Collections.Immutable;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public class ChunkManager : ChunkProvider, IDisposable
{
    
    private Dictionary<Vector3D<int>, Chunk> chunks;
    private object chunksLock = new object();
    public WorldGenerator worldGenerator { get;  set; }


    private struct ChunkLoadingTask
    {
        public Chunk chunk { get; set; }
        public ChunkState chunkState { get; set; }

        public ChunkLoadingTask(Chunk chunk, ChunkState chunkState) {
            this.chunk = chunk;
            this.chunkState = chunkState;
        }
    }
    
    private List<ChunkLoadingTask> chunksToLoad;
    private Thread chunkLoaderThread;
    private bool runThread = true;
    private SemaphoreSlim semaphore;
    
    
    
    public ChunkManager(int RADIUS, WorldGenerator worldGenerator) {
        chunks = new  Dictionary<Vector3D<int>, Chunk>(RADIUS * RADIUS * RADIUS);
        this.worldGenerator = worldGenerator;
        chunksToLoad = new List<ChunkLoadingTask>();
        semaphore = new SemaphoreSlim(0);
        chunkLoaderThread = new Thread(chunkLoaderThreadRuntime);
        chunkLoaderThread.Start();
    }

    public void chunkLoaderThreadRuntime() {
        while (runThread) {
            semaphore.Wait();
            ChunkLoadingTask task;
            if(chunksToLoad.Count <= 0) continue;
            task = chunksToLoad.First();
            chunksToLoad.RemoveAt(0);
            task.chunk.setMinimumWantedChunkState(task.chunkState);
        }
    }


    public void update() {
        if (chunksToLoad.Count <= 0) return;
        if (semaphore.CurrentCount <= 0) {
            semaphore.Release(1000);
        }
    }

    public int count() => chunks.Count;
    public ImmutableDictionary<Vector3D<int>,Chunk> getImmutableDictionary() => chunks.ToImmutableDictionary();
    public bool ContainsKey(Vector3D<int> position) => chunks.ContainsKey(position);
    
    public IEnumerable<Chunk> getChunks() {
        lock (chunksLock) {
            List<Chunk> chunksToIterate = chunks.Values.ToList();
            foreach (var chunkToIterate in chunksToIterate) {
                yield return chunkToIterate;
            }
        }
    }

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

    public void addOrSet(Chunk chunk) {
        lock (chunksLock) {
            if (chunks.ContainsKey(chunk.position)) {
                chunks[chunk.position] = chunk;
            } else {
                chunks.Add(chunk.position, chunk);
            }
        }
    }
    
    private void removeChunk(Chunk chunk) {
        chunk.Dispose();
        chunks.Remove(chunk.position);
    }

    public void Dispose() {
        runThread = false;
        chunksToLoad.Clear();
        semaphore.Release(1);
        chunkLoaderThread.Join();
    }
}