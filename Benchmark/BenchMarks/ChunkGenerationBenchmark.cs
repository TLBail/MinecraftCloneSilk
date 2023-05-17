using BenchmarkDotNet.Attributes;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace Benchmark.BenchMarks;

[MemoryDiagnoser,KeepBenchmarkFiles]
public class ChunkGenerationBenchmark
{
    private Game game;
    private Thread gameThread;

    private IChunkManager chunkManager;
    
    [GlobalSetup]
    public async void Setup() {
        Scene scene = new Scene(new List<InitGameData>()
            {
                new (typeof(Player).FullName),
                new (typeof(World).FullName, new object[]{WorldMode.EMPTY}),
                new (typeof(Console).FullName)
            }
        );
        game = Game.getInstance(scene, false);
        gameThread = new Thread(() => {
            game.Run();
        });
        gameThread.Start();
        await game.waitForFrame(1);
    }

    [GlobalCleanup]
    public async void cleanUp() {
        await game.waitForFrame(10);
        game.Stop();
        gameThread.Join();
    }
    
    
    [Benchmark]
    public void createAllBlockForAChunk() {
        Chunk chunk = new Chunk(Vector3D<int>.Zero, null, null, null);
    }
    
}