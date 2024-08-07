﻿namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkTerrainGeneratedStrategy : ChunkStrategy
{

    public ChunkTerrainGeneratedStrategy(Chunk chunk) : base(chunk) {
        if (chunk.chunkState != ChunkState.EMPTY && chunk.chunkState != ChunkState.GENERATEDTERRAIN && chunk.chunkState != ChunkState.STORAGELOADING) {
            throw new Exception("try to create a ChunkTerrainGeneratedStrategy with a chunk that is not empty");
        }
    }

    
    public override void Init() {
        chunk.chunkState = ChunkState.TERRAINLOADING;
    }

    public override void Load() {
        GenerateTerrain();
    }

    public override void Finish() {
        chunk.chunkState = ChunkState.GENERATEDTERRAIN;
    }
    
    
    public override ChunkState GetChunkStateOfStrategy() => ChunkState.GENERATEDTERRAIN;
    
    public override Block GetBlock(int x, int y, int z) {
        throw new Exception("try to get Block of a terrain generated only chunk");
    }

    private void GenerateTerrain()
    {
        chunk.worldGenerator.GenerateTerrain(chunk.position, chunk.blocks);
    }
}