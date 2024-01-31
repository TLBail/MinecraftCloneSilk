using BenchmarkDotNet.Attributes;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;
using UnitTest.fakeClass;

namespace Benchmark.BenchMarks;

[MemoryDiagnoser]
public class ChunkLoadingBenchmark
{
    private ChunkManagerEmpty chunkManagerEmpty = null!;
    private ChunkLoader chunkLoader = null!;
    [IterationSetup]
    public void GlobalSetup() {
        Directory.SetCurrentDirectory("./../../../../../../../../");
        Chunk.InitStaticMembers(null, BlockFactory.GetInstance());
        chunkManagerEmpty = new ChunkManagerEmpty(new WorldFlatGeneration(), new NullChunkStorage());
        chunkLoader = new ChunkLoader(ChunkLoader.ChunkLoaderMode.SYNC);
    }
    
    [Benchmark]
    public void LoadChunk() {
        for (int i = 0; i < 1000; i++) {
            ChunkManagerTools.GetBlockGeneratedChunk(chunkManagerEmpty, chunkLoader, Vector3D<int>.UnitX * i * 100);
        }
    }
 
}