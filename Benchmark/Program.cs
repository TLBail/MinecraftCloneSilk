using System.Text;
using Benchmark.BenchMarks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

class Program
{
    static void Main(string[] args) {
        MetricTest.test();
    }
}
