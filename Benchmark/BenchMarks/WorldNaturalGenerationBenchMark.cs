using BenchmarkDotNet.Attributes;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;

namespace Benchmark.BenchMarks;

[MemoryDiagnoser,KeepBenchmarkFiles]
public class WorldNaturalGenerationBenchMark
{
    public int bob = 0;
    public static int dagniel = 0; 
    WorldNaturalGeneration worldNaturalGeneration;
    BlockData[,,] blocks;
    Vector3D<int> position = Vector3D<int>.Zero;
    public WorldNaturalGenerationBenchMark() {
        blocks = new BlockData[16, 16, 16];
        worldNaturalGeneration = new WorldNaturalGeneration();
    }

    [Benchmark]
    public void createAllBlockForAChunk() {
        worldNaturalGeneration.GenerateTerrain(position, blocks);
    }
    
}