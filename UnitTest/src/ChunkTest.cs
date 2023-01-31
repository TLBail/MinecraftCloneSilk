﻿using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;
using UnitTest.fakeClass;

namespace UnitTest;

public class ChunkTest
{
    [SetUp]
    public void setUp() {
        TextureManager textureManager = TextureManager.getInstance();
        textureManager.fakeLoad();
    }
    
    [Test]
    public void testChunkIsNotNull() {
        ChunkProviderEmpty chunkProviderEmpty = new ChunkProviderEmpty(new WorldFlatGeneration());
        Chunk chunk = chunkProviderEmpty.getChunk(Vector3D<int>.Zero);
        Assert.NotNull(chunk);
    }
    
    [Test]
    public void testChunkLoadTerrain() {
        ChunkProviderEmpty chunkProviderEmpty = new ChunkProviderEmpty(new WorldFlatGeneration());
        Chunk chunk = chunkProviderEmpty.getChunk(Vector3D<int>.Zero);
        chunk.setWantedChunkState(ChunkState.Generatedterrain);
        Block block = chunk.getBlock(Vector3D<int>.Zero);
        Assert.IsNotNull(block);
        Assert.True(block.name.Equals("grass"));
    }
    
    

}