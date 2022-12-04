using System.Text;
using Benchmark.BenchMarks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<ChunkGenerationBenchmark>();
    }
}
