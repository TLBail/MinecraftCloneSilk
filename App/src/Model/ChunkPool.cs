﻿using System.Collections.Concurrent;
using System.Diagnostics;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.Lighting;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public class ChunkPool
{

    private readonly ConcurrentBag<Chunk> chunkPool = new ConcurrentBag<Chunk>();

    public IChunkManager chunkManager { get; init; }
    public IWorldGenerator worldGenerator { get; init; }
    public IChunkStorage chunkStorage { get; init; }
    public IChunkLightManager chunkLightManager { get; init; }
    
    public ChunkPool(IChunkManager chunkManager, IWorldGenerator worldGenerator, IChunkStorage chunkStorage, IChunkLightManager chunkLightManager) {    
        this.chunkManager = chunkManager;
        this.worldGenerator = worldGenerator;
        this.chunkStorage = chunkStorage;
        this.chunkLightManager = chunkLightManager;
    }

    public int Count() => chunkPool.Count;
    public Chunk Get(Vector3D<int> position) {
        Chunk chunk;
        if (chunkPool.TryTake(out Chunk? result)) {
            chunk = result;
            chunk.Reset(position, chunkManager, worldGenerator, chunkStorage, chunkLightManager);
        } else {
            chunk = BuildChunk(position);
        }

        return chunk;
    }

    public void ReturnChunk(Chunk chunk) {
        Debug.Assert(!chunk.IsRequiredByChunkLoader(), " chunk is still required by chunk loader");
        Debug.Assert(!chunk.blockModified, " chunk still have block modified");
        Debug.Assert(!chunk.IsRequiredByChunkSaver());
        chunkPool.Add(chunk);  
    } 
    
    private Chunk BuildChunk(Vector3D<int> position) {
        return new Chunk(position, chunkManager, worldGenerator, chunkStorage, chunkLightManager);
    }
}