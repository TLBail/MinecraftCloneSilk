﻿using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.Model.Chunk;

public abstract class ChunkStrategy
{
    public abstract ChunkState getChunkStateOfStrategy();
    
    protected Chunk chunk;
    
    public ChunkStrategy(Chunk chunk) {
        this.chunk = chunk;
    }
    
    public virtual BlockData getBlockData(Vector3D<int> localPosition) {
        if (localPosition.Y < 0) {
            return chunk.chunksNeighbors[(int)Face.BOTTOM]!
                .getBlockData(new Vector3D<int>(localPosition.X, localPosition.Y + (int)Chunk.CHUNK_SIZE, localPosition.Z));
        }else if (localPosition.Y >= Chunk.CHUNK_SIZE) {
            return chunk.chunksNeighbors[(int)Face.TOP]!
                .getBlockData(new Vector3D<int>(localPosition.X, localPosition.Y - (int)Chunk.CHUNK_SIZE, localPosition.Z));
        }else if (localPosition.X < 0) {
            return chunk.chunksNeighbors[(int)Face.LEFT]!
                .getBlockData(new Vector3D<int>(localPosition.X + (int)Chunk.CHUNK_SIZE, localPosition.Y, localPosition.Z));
        }else if (localPosition.X >= Chunk.CHUNK_SIZE) {
            return chunk.chunksNeighbors[(int)Face.RIGHT]!
                .getBlockData(new Vector3D<int>(localPosition.X - (int)Chunk.CHUNK_SIZE, localPosition.Y, localPosition.Z));
        } else if (localPosition.Z < 0) {
            return chunk.chunksNeighbors[(int)Face.BACK]!
                .getBlockData(new Vector3D<int>(localPosition.X, localPosition.Y, localPosition.Z + (int)Chunk.CHUNK_SIZE));
        }else if (localPosition.Z >= Chunk.CHUNK_SIZE) {
            return chunk.chunksNeighbors[(int)Face.FRONT]!
                .getBlockData(new Vector3D<int>(localPosition.X, localPosition.Y, localPosition.Z - (int)Chunk.CHUNK_SIZE));
        } else {
            return chunk.blocks[localPosition.X, localPosition.Y, localPosition.Z];
        }
    }

    public virtual Task updateChunkVertex() {
        throw new Exception("try to update Chunk Vertex on a non initialized chunk");
    }
    
    public virtual void draw(GL gl, double deltaTime){}

    public abstract void setBlock(int x, int y, int z, string name);

    public virtual async Task<Block> getBlock(int x, int y, int z) {
        await chunk.setMinimumWantedChunkState(ChunkState.BLOCKGENERATED);
        var blockData = getBlockData(new Vector3D<int>(x, y, z));
        return Chunk.blockFactory.buildFromBlockData(new Vector3D<int>(x, y, z), blockData);
    }
    protected async Task updateNeighboorChunkState(ChunkState chunkState) {
        foreach (Face face in Enum.GetValues(typeof(Face))) {
            Chunk newChunk = chunk.chunkProvider.getChunk(chunk.position + (FaceOffset.getOffsetOfFace(face) * 16));
            await newChunk.setMinimumWantedChunkState(chunkState);
            chunk.chunksNeighbors[(int)face] = newChunk;
        }
    }

    public virtual void Dispose(){}
    
    public virtual void update(double deltaTime){}
    
    public async virtual Task init(){}
    
    
}