using System.Text;
using Benchmark.BenchMarks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

class Program
{
    static void Main(string[] args) {
        BenchmarkRunner.Run<TryToUnloadChunkBenchmark>();
        
    }


    public static void runChunkIOBenchmark() {
        BenchmarkRunner.Run<ChunkIOBenchmark>();
    }
    
    public static void runBenchmark() { 
        //BenchmarkRunner.Run<ChunkGenerationBenchmark>();
        BenchmarkRunner.Run<WorldNaturalGenerationBenchMark>();
    }
}
