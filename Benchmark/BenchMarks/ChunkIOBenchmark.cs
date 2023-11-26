using BenchmarkDotNet.Attributes;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using MinecraftCloneSilk.Model.WorldGen;
using NUnit.Framework;
using Silk.NET.Maths;
using UnitTest.fakeClass;

namespace Benchmark.BenchMarks;

[MemoryDiagnoser]
public class ChunkIOBenchmark
{
    private ChunkManagerEmpty chunkManagerEmpty = null!;
    private ChunkLoader chunkLoader = null!;
    private RegionStorage regionStorage = null!;
    
    [GlobalSetup]
    public void GlobalSetup() {
        Directory.SetCurrentDirectory("./../../../../../../../../");
        Chunk.InitStaticMembers(null, BlockFactory.GetInstance());
        regionStorage = new RegionStorage("./Worlds/newWorld");
        chunkManagerEmpty = new ChunkManagerEmpty(new WorldFlatGeneration(), regionStorage);
        chunkLoader = new ChunkLoader(ChunkLoader.ChunkLoaderMode.SYNC);
    }

     

    private List<Chunk> chunks = null!;
    [IterationSetup]
    public void Setup() {
        regionStorage.Clear(); 

        chunks = new();
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++) {
                for (int k = 0; k < Chunk.CHUNK_SIZE; k++) {
                    Chunk chunk = ChunkManagerTools.GetBlockGeneratedChunk(chunkManagerEmpty, chunkLoader,
                        new Vector3D<int>(i * Chunk.CHUNK_SIZE, j * Chunk.CHUNK_SIZE, k * Chunk.CHUNK_SIZE));
                    chunks.Add(chunk);
                }
            }
        }
        
    }

    [Benchmark]
    public void SaveChunk() {
        regionStorage.SaveChunks(chunks);
    }
    

    
    


}