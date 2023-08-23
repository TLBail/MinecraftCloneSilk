using BenchmarkDotNet.Attributes;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using NUnit.Framework;
using Silk.NET.Maths;
using UnitTest.fakeClass;

namespace Benchmark.BenchMarks;

[MemoryDiagnoser]
public class ChunkIOBenchmark
{
    private ChunkManagerEmpty chunkManagerEmpty;
    private ChunkLoader chunkLoader;
    private RegionStorage regionStorage;
    
    [GlobalSetup]
    public void globalSetup() {
        Directory.SetCurrentDirectory("./../../../../../../../../");
        Chunk.initStaticMembers(null, BlockFactory.getInstance());
        regionStorage = new RegionStorage("./Worlds/newWorld");
        chunkManagerEmpty = new ChunkManagerEmpty(new WorldFlatGeneration(), regionStorage);
        chunkLoader = new ChunkLoader(regionStorage);
    }

     
    public void loadChunk() {
        getBlockGeneratedChunk(Vector3D<int>.Zero);
    }

    private List<Chunk> chunks;
    [IterationSetup]
    public void setup() {
        regionStorage.Clear(); 

        chunks = new();
        for (int i = 0; i < 16; i++) {
            for (int j = 0; j < 16; j++) {
                for (int k = 0; k < 16; k++) {
                    chunks.Add(getBlockGeneratedChunk(new Vector3D<int>(i * Chunk.CHUNK_SIZE, j * Chunk.CHUNK_SIZE, k * Chunk.CHUNK_SIZE)));
                }
            }
        }
        
    }

    [Benchmark]
    public void saveChunk() {
        regionStorage.SaveChunks(chunks);
    }
    

    
    
    private Chunk getBlockGeneratedChunk(Vector3D<int> position) {
        Chunk chunk = chunkManagerEmpty.getChunk(position);
        Stack<ChunkLoadingTask> chunkLoadingTasks = new Stack<ChunkLoadingTask>();
        chunkLoadingTasks.Push(new ChunkLoadingTask(chunk, ChunkState.BLOCKGENERATED));
        chunkLoader.addChunks(ChunkManagerTools.getChunkDependent(chunkManagerEmpty, chunkLoadingTasks));
        chunkLoader.singleThreadLoading();
        return chunk;
    }


}