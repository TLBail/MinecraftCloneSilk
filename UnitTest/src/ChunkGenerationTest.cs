using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
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
        Directory.SetCurrentDirectory("./../../../../");
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
    [SetUp]
    public async Task setup() {
        await game.waitForFrame(1);
    }

    [TearDown]
    public async Task tearDown() {
        await game.waitForFrame(10);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.clear();
        await game.waitForFrame(10);
    }
    
    [OneTimeTearDown]
    public void endGame() {
        game.Stop();
        gameThread.Join();
        
        DirectoryInfo directory = Directory.CreateDirectory("./Worlds/newWorld");
        foreach (var file in         Directory.GetFiles("./Worlds/newWorld")) {
            File.Delete(file);
        }
    }
    
    

    [Test]
    public async Task TestOneChunkGeneration() {
         await game.waitForFrame(1);
         world = (World)game.gameObjects[typeof(World).FullName];
         world.chunkManager.addChunksToLoad(new List<Vector3D<int>>(){Vector3D<int>.Zero});
         await game.waitForFrame(3);
         Assert.NotNull( world.getBlock(Vector3D<int>.Zero));
    }
    
    [Test]
    public async Task TestDeletingBlock() {
        await game.waitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.addChunksToLoad(new List<Vector3D<int>>(){Vector3D<int>.Zero});
        await game.waitForFrame(10);
        world.setBlock(BlockFactory.AIR_BLOCK, Vector3D<int>.Zero);
        var block = world.getBlock(Vector3D<int>.Zero);
        Assert.True(block.name.Equals(BlockFactory.AIR_BLOCK));
    }


    [Test]
    public async Task TestUnloading() {
        await game.waitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.addChunksToLoad(new List<Vector3D<int>>(){Vector3D<int>.Zero});
        await game.waitForFrame(10);
        world.setBlock(BlockFactory.AIR_BLOCK, Vector3D<int>.Zero);
        var block = world.getBlock(Vector3D<int>.Zero);
        
        world.chunkManager.tryToUnloadChunk(Vector3D<int>.Zero);
        await game.waitForFrame(1);
        Assert.False(world.chunkManager.getChunksList().Any((chunk) => chunk.position.Equals(Vector3D<int>.Zero)));
    }
    
    
    [Test]
    public async Task TestChunkHaveBeenSave() {
        await game.waitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.addChunksToLoad(new List<Vector3D<int>>(){Vector3D<int>.Zero});
        await game.waitForFrame(10);
        world.setBlock(BlockFactory.AIR_BLOCK, Vector3D<int>.Zero);
        var block = world.getBlock(Vector3D<int>.Zero);
        
        world.chunkManager.tryToUnloadChunk(Vector3D<int>.Zero);
        await game.waitForFrame(10);
        
        world.chunkManager.addChunksToLoad(new List<Vector3D<int>>(){Vector3D<int>.Zero});
        await game.waitForFrame(3);
        Assert.True(world.getBlock(Vector3D<int>.Zero).name.Equals(BlockFactory.AIR_BLOCK));
    }


    [Test]
    public async Task TestChunkSaveWhenblockGenerated() {
        await game.waitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.addChunksToLoad(new List<Vector3D<int>>()
        {
            Vector3D<int>.Zero,
            new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0),
        });
        await game.waitForFrame(10);
        world.setBlock(BlockFactory.AIR_BLOCK, new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0));
        
        world.chunkManager.tryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0));
        await game.waitForFrame(10);
        
        world.chunkManager.addChunksToLoad(new List<Vector3D<int>>(){new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0)});
        await game.waitForFrame(10);
        Assert.True(world.getBlock(new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0)).name.Equals(BlockFactory.AIR_BLOCK));

    }
    
    
    [Test]
    public async Task TestChunkSaveWhenEmpty() {
        await game.waitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.addChunksToLoad(new List<Vector3D<int>>()
        {
            Vector3D<int>.Zero,
            new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0),
            new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0),

        });
        await game.waitForFrame(10);
        world.setBlock("metal", new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0));
        Assert.That(world.getBlock(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0)).name.Equals("metal"), Is.True);
        
        world.chunkManager.tryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0));
        await game.waitForFrame(10);
        Assert.That(world.chunkManager.getChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0)).chunkState, Is.EqualTo(ChunkState.EMPTY));
        
        world.chunkManager.tryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0));
        await game.waitForFrame(10);
        Assert.True(world.chunkManager.getChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0)).chunkState == ChunkState.EMPTY);

        world.chunkManager.tryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0));
        await game.waitForFrame(10);

        Assert.False(world.chunkManager.getChunksList().Any((chunk) => chunk.position.Equals(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0))));

        world.chunkManager.addChunksToLoad(new List<Vector3D<int>>(){new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0)});
        await game.waitForFrame(10);
        
        
        Assert.That(world.getBlock(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0)).name.Equals("metal"), Is.True);

    }
    
    [Test]
    public async Task TestVeryFareChunkSaveWhenEmpty() {
        await game.waitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.addChunksToLoad(new List<Vector3D<int>>()
        {
            Vector3D<int>.Zero,
            new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0),
            new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0),
            new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0),
        });
        await game.waitForFrame(10);
        world.setBlock("metal", new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0));

        
        world.chunkManager.tryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0));
        await game.waitForFrame(10);
        Assert.That(world.chunkManager.getChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0)).chunkState, Is.EqualTo(ChunkState.EMPTY));
        
        world.chunkManager.tryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0));
        await game.waitForFrame(10);
        Assert.That(world.chunkManager.getChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0)).chunkState, Is.EqualTo(ChunkState.EMPTY));
        
        world.chunkManager.tryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0));
        await game.waitForFrame(10);

        Assert.False(world.chunkManager.getChunksList().Any((chunk) => chunk.position.Equals(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0))));
        
        
        world.chunkManager.tryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0));
        await game.waitForFrame(10);
        Assert.That(world.chunkManager.getChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0)).chunkState, Is.EqualTo(ChunkState.EMPTY));

        world.chunkManager.tryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0));
        await game.waitForFrame(10);
        Assert.False(world.chunkManager.getChunksList().Any((chunk) => chunk.position.Equals(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0))));

        world.chunkManager.addChunksToLoad(new List<Vector3D<int>>(){new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0)});
        await game.waitForFrame(10);
        Assert.That(world.getBlock(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0)).name, Is.EqualTo("metal"));

    }
}