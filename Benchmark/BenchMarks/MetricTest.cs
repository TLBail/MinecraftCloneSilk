using System.Diagnostics;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Timer;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;
using Console = MinecraftCloneSilk.UI.Console;

namespace Benchmark.BenchMarks;

public class MetricTest
{
    public static async Task test() {
        var metrics = new MetricsBuilder()
            .Report.ToConsole()
            .Build();

        Scene scene = new Scene(new List<InitGameData>()
            {
                new (typeof(Player).FullName),
                new (typeof(World).FullName, new object[]{WorldMode.EMPTY}),
                new (typeof(Console).FullName)
            }
        );
        Game game = Game.getInstance(scene, false);
        Thread gameThread = new Thread(() => {
            game.Run();
        });
        gameThread.Start();
        await game.waitForFrame(1);

        await mesureGame(metrics, game);
        
        game.Stop();

        await Task.WhenAll(metrics.ReportRunner.RunAllAsync());

    }

    private static async Task mesureGame(IMetricsRoot metrics, Game game) {
        World world = game.gameObjects[typeof(World).FullName] as World;
        TimerOptions timerOptions = new TimerOptions{ Name = "chunk terrain creation"};
        Block block = null;
        Chunk chunk = world.getChunk(Vector3D<int>.Zero);
        using (metrics.Measure.Timer.Time(timerOptions)) {
            chunk.setWantedChunkState(ChunkState.GENERATEDTERRAIN);
        }
        timerOptions = new TimerOptions{ Name = "chunk block creation"};
        using (metrics.Measure.Timer.Time(timerOptions)) {
            chunk.setWantedChunkState(ChunkState.BLOCKGENERATED);
        }
        timerOptions = new TimerOptions{ Name = "chunk drawable creation"};
        using (metrics.Measure.Timer.Time(timerOptions) ){
            chunk.setWantedChunkState(ChunkState.DRAWABLE);
        }
        

        System.Console.WriteLine("block  " + chunk.getBlockData(Vector3D<int>.Zero));
        
        
    }
}