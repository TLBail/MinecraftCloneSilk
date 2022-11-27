using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.UI;
using NUnit.Framework.Interfaces;
using Silk.NET.Maths;

namespace UnitTest;

[MemoryDiagnoser]
public class ChunkGenerationTest
{
    private Game game;
    private Thread gameThread;
    private World world;
    
    private Scene fullScene = new Scene(new List<InitGameData>()
    {
        new(typeof(Player).FullName),
        new(typeof(World).FullName, new object[] { WorldMode.DYNAMIC }),
        new(typeof(GameUi).FullName),
        new(typeof(GeneralInfo).FullName)
    });
    
    [OneTimeSetUp]
    public  void initGame() {
        Scene scene = new Scene(new List<InitGameData>()
            {
                new (typeof(Player).FullName),
                new (typeof(World).FullName, new object[]{WorldMode.EMPTY}),
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
        Assert.True(world.getBlock(Vector3D<int>.Zero).name.Equals(BlockFactory.AIR_BLOCK));
    }

    
}