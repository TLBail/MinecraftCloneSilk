using System.Text;
using Benchmark.BenchMarks;
using BenchmarkDotNet.Running;

class Program
{
    static void Main(string[] args) {
        BenchmarkRunner.Run<ChunkLoadingBenchmark>();
    }

}
