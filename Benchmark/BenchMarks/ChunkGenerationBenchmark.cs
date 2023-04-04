using BenchmarkDotNet.Attributes;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;
using UnitTest.fakeClass;

namespace Benchmark.BenchMarks;

[MemoryDiagnoser,KeepBenchmarkFiles]
public class ChunkGenerationBenchmark
{
    public int bob = 0;
    public static int dagniel = 0; 
    WorldNaturalGeneration worldNaturalGeneration;
    BlockData[,,] blocks;
    Vector3D<int> position = Vector3D<int>.Zero;
    private IChunkManager chunkManager;
    public ChunkGenerationBenchmark() {
        TextureManager textureManager = TextureManager.getInstance();
        textureManager.fakeLoad();
        blocks = new BlockData[16, 16, 16];
        worldNaturalGeneration = new WorldNaturalGeneration();
        chunkManager = new ChunkManagerEmpty(worldNaturalGeneration);
    }

    [Benchmark]
    public void createAllBlockForAChunk() {
        Chunk chunk = new Chunk(position, chunkManager, worldNaturalGeneration, null);
    }
    
}