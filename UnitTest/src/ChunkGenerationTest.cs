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
                new (typeof(Player).FullName!),
                new (typeof(World).FullName!, new object[]{WorldMode.EMPTY}),
                new (typeof(Console).FullName!)
            }
        );
        game = Game.GetInstance(scene, false);
        gameThread = new Thread(() => {
            game.Run();
        });
        gameThread.Start();
    }
    [SetUp]
    public async Task Setup() {
        await game.WaitForFrame(1);
    }

    [TearDown]
    public async Task TearDown() {
        await game.WaitForFrame(10);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.Clear();
        await game.WaitForFrame(10);
    }
    
    [OneTimeTearDown]
    public void EndGame() {
        game.Stop();
        gameThread.Join();
    }
    
    

    [Test]
    public async Task TestOneChunkGeneration() {
         await game.WaitForFrame(1);
         world = (World)game.gameObjects[typeof(World).FullName];
         world.chunkManager.AddChunksToLoad(new List<Vector3D<int>>(){Vector3D<int>.Zero});
         await game.WaitForFrame(3);
         Assert.NotNull( world.GetBlock(Vector3D<int>.Zero));
    }
    
    [Test]
    public async Task TestDeletingBlock() {
        await game.WaitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.AddChunksToLoad(new List<Vector3D<int>>(){Vector3D<int>.Zero});
        await game.WaitForFrame(10);
        world.SetBlock(BlockFactory.AIR_BLOCK, Vector3D<int>.Zero);
        var block = world.GetBlock(Vector3D<int>.Zero);
        Assert.True(block.name.Equals(BlockFactory.AIR_BLOCK));
    }


    [Test]
    public async Task TestUnloading() {
        await game.WaitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.AddChunksToLoad(new List<Vector3D<int>>(){Vector3D<int>.Zero});
        await game.WaitForFrame(10);
        world.SetBlock(BlockFactory.AIR_BLOCK, Vector3D<int>.Zero);
        var block = world.GetBlock(Vector3D<int>.Zero);
        
        world.chunkManager.TryToUnloadChunk(Vector3D<int>.Zero);
        await game.WaitForFrame(1);
        Assert.False(world.chunkManager.GetChunksList().Any((chunk) => chunk.position.Equals(Vector3D<int>.Zero)));
    }
    
    
    [Test]
    public async Task TestChunkHaveBeenSave() {
        await game.WaitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.AddChunksToLoad(new List<Vector3D<int>>(){Vector3D<int>.Zero});
        await game.WaitForFrame(10);
        world.SetBlock(BlockFactory.AIR_BLOCK, Vector3D<int>.Zero);
        var block = world.GetBlock(Vector3D<int>.Zero);
        
        world.chunkManager.TryToUnloadChunk(Vector3D<int>.Zero);
        await game.WaitForFrame(10);
        
        world.chunkManager.AddChunksToLoad(new List<Vector3D<int>>(){Vector3D<int>.Zero});
        await game.WaitForFrame(3);
        Assert.True(world.GetBlock(Vector3D<int>.Zero).name.Equals(BlockFactory.AIR_BLOCK));
    }


    [Test]
    public async Task TestChunkSaveWhenblockGenerated() {
        await game.WaitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.AddChunksToLoad(new List<Vector3D<int>>()
        {
            Vector3D<int>.Zero,
            new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0),
        });
        await game.WaitForFrame(10);
        world.SetBlock(BlockFactory.AIR_BLOCK, new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0));
        
        world.chunkManager.TryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0));
        await game.WaitForFrame(10);
        
        world.chunkManager.AddChunksToLoad(new List<Vector3D<int>>(){new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0)});
        await game.WaitForFrame(10);
        Assert.True(world.GetBlock(new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0)).name.Equals(BlockFactory.AIR_BLOCK));

    }
    
    
    [Test]
    public async Task TestChunkSaveWhenEmpty() {
        await game.WaitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.AddChunksToLoad(new List<Vector3D<int>>()
        {
            Vector3D<int>.Zero,
            new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0),
            new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0),

        });
        await game.WaitForFrame(10);
        world.SetBlock("metal", new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0));
        Assert.That(world.GetBlock(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0)).name.Equals("metal"), Is.True);
        
        world.chunkManager.TryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0));
        await game.WaitForFrame(10);
        Assert.That(world.chunkManager.GetChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0)).chunkState, Is.EqualTo(ChunkState.EMPTY));
        
        world.chunkManager.TryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0));
        await game.WaitForFrame(10);
        Assert.True(world.chunkManager.GetChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0)).chunkState == ChunkState.EMPTY);

        world.chunkManager.TryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0));
        await game.WaitForFrame(10);

        Assert.False(world.chunkManager.GetChunksList().Any((chunk) => chunk.position.Equals(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0))));

        world.chunkManager.AddChunksToLoad(new List<Vector3D<int>>(){new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0)});
        await game.WaitForFrame(10);
        
        
        Assert.That(world.GetBlock(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0)).name.Equals("metal"), Is.True);

    }
    
    [Test]
    public async Task TestVeryFareChunkSaveWhenEmpty() {
        await game.WaitForFrame(1);
        world = (World)game.gameObjects[typeof(World).FullName];
        world.chunkManager.AddChunksToLoad(new List<Vector3D<int>>()
        {
            Vector3D<int>.Zero,
            new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0),
            new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0),
            new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0),
        });
        await game.WaitForFrame(10);
        world.SetBlock("metal", new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0));

        
        world.chunkManager.TryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0));
        await game.WaitForFrame(10);
        Assert.That(world.chunkManager.GetChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0)).chunkState, Is.EqualTo(ChunkState.EMPTY));
        
        world.chunkManager.TryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0));
        await game.WaitForFrame(10);
        Assert.That(world.chunkManager.GetChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0)).chunkState, Is.EqualTo(ChunkState.EMPTY));
        
        world.chunkManager.TryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0));
        await game.WaitForFrame(10);

        Assert.False(world.chunkManager.GetChunksList().Any((chunk) => chunk.position.Equals(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0))));
        
        
        world.chunkManager.TryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0));
        await game.WaitForFrame(10);
        Assert.That(world.chunkManager.GetChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0)).chunkState, Is.EqualTo(ChunkState.EMPTY));

        world.chunkManager.TryToUnloadChunk(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0));
        await game.WaitForFrame(10);
        Assert.False(world.chunkManager.GetChunksList().Any((chunk) => chunk.position.Equals(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 2, 0, 0))));

        world.chunkManager.AddChunksToLoad(new List<Vector3D<int>>(){new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0)});
        await game.WaitForFrame(10);
        Assert.That(world.GetBlock(new Vector3D<int>((int)Chunk.CHUNK_SIZE * 3, 0, 0)).name, Is.EqualTo("metal"));

    }
}