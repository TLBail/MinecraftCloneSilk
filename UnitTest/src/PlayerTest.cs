using System.Numerics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model;
using Silk.NET.Input;
using Silk.NET.Maths;
using Console = MinecraftCloneSilk.UI.Console;

namespace UnitTest;

public class PlayerTest
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
                new (typeof(Console).FullName),
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
    
    [SetUp]
    public async Task setUp() {
        await game.waitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.addChunkToLoad(Vector3D<int>.Zero);
        await game.waitForFrame(3);
        
    }

    [TearDown]
    public async Task tearDown() {
        await game.waitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.clear();
        await game.waitForFrame(10);

    }

    [Test]
    public async Task playerRemoveBlockWithClick() {
        await game.waitForFrame(1);
        Player player = (Player)game.gameObjects[typeof(Player).FullName];
        world = (World)game.gameObjects[typeof(World).FullName];
        
        world.setBlock("stone", Vector3D<int>.Zero);
        Assert.True((world.getBlock(Vector3D<int>.Zero)).name.Equals("stone"));
        player.position = Vector3.Zero;
        player.onMouseClick(null, MouseButton.Left);
        Assert.True((world.getBlock(Vector3D<int>.Zero)).name.Equals(BlockFactory.AIR_BLOCK));
    }
    
    [Test]
    public async Task playerAddBlockWithClick() {
        await game.waitForFrame(1);
        Player player = (Player)game.gameObjects[typeof(Player).FullName];
        world = (World)game.gameObjects[typeof(World).FullName];
        world.setBlock("stone", Vector3D<int>.Zero);
        Assert.True((world.getBlock(Vector3D<int>.Zero)).name.Equals("stone"));
        player.position = Vector3.Zero;
        player.onMouseClick(null, MouseButton.Left);
        Assert.True((world.getBlock(Vector3D<int>.Zero)).name.Equals(BlockFactory.AIR_BLOCK));
    }

}