﻿using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkEmptyStrategy : ChunkStrategy
{
    public ChunkEmptyStrategy(Chunk chunk) : base(chunk) {
    }

    public override ChunkState GetChunkStateOfStrategy() => ChunkState.EMPTY;


    public override BlockData GetBlockData(Vector3D<int> localPosition) {
        throw new Exception("try to access to block data but the chunk is empty");
    }

    public override void SetBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = Chunk.blockFactory!.GetBlockIdByName(name);
    }

    public override Block GetBlock(int x, int y, int z) {
        throw new Exception("try to get Block of a empty chunk");
    }
}