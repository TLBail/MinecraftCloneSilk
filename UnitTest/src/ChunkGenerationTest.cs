using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.UI;
using NUnit.Framework.Interfaces;
using Silk.NET.Maths;
using Console = MinecraftCloneSilk.UI.Console;

namespace UnitTest;

public class ChunkGenerationTest
{
    private Game game;
    private Thread gameThread;
    private World world;

    [OneTimeSetUp]
    public  void initGame() {
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
    }

    [OneTimeTearDown]
    public void endGame() {
        game.Stop();
        gameThread.Join();
    }
    
    [Test]
    public async Task testOneChunkGeneration() {
         await game.waitForFrame(1);
         world = (World)game.gameObjects[typeof(World).FullName];
         Assert.NotNull( world.getBlock(Vector3D<int>.Zero));
    }
    
    [Test]
    public async Task testDeletingBlock() {
        await game.waitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.setBlock(BlockFactory.AIR_BLOCK, Vector3D<int>.Zero);
        var task = world.getBlock(Vector3D<int>.Zero);
        task.Wait();
        Assert.True(task.Result.name.Equals(BlockFactory.AIR_BLOCK));
    }

    
}