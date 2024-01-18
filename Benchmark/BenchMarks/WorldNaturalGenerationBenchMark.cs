using BenchmarkDotNet.Attributes;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;

namespace Benchmark.BenchMarks;

[MemoryDiagnoser,KeepBenchmarkFiles]
public class WorldNaturalGenerationBenchMark
{
    public int bob = 0;
    public static int dagniel = 0; 
    WorldNaturalGeneration worldNaturalGeneration;
    private ChunkData chunkData;
    Vector3D<int> position = Vector3D<int>.Zero;
    public WorldNaturalGenerationBenchMark() {
        chunkData = new ChunkData();
        worldNaturalGeneration = new WorldNaturalGeneration();
    }

    [Benchmark]
    public void createAllBlockForAChunk() {
        worldNaturalGeneration.GenerateTerrain(position, chunkData);
    }
    
}