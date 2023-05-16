﻿using System.IO.Compression;
using MinecraftCloneSilk.Core;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.Model.NChunk;

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
    

    public virtual void updateChunkVertex() {}
    
    public virtual void draw(GL gl, double deltaTime){}

    public abstract void setBlock(int x, int y, int z, string name);

    public virtual Block getBlock(int x, int y, int z) {
        chunk.setMinimumWantedChunkState(ChunkState.BLOCKGENERATED);
        var blockData = getBlockData(new Vector3D<int>(x, y, z));
        return Chunk.blockFactory.buildFromBlockData(new Vector3D<int>(x, y, z), blockData);
    }
    protected virtual void updateNeighboorChunkState(ChunkState chunkState) {
        lock (chunk.chunksNeighborsLock) {
            if (chunk.chunksNeighbors.Length != 6) {
                chunk.chunksNeighbors = new Chunk?[]
                {
                    chunk.chunksNeighbors[0],
                    chunk.chunksNeighbors[1],
                    chunk.chunksNeighbors[2],
                    chunk.chunksNeighbors[3],
                    chunk.chunksNeighbors[4],
                    chunk.chunksNeighbors[5]
                };
            }
            foreach (Face face in Enum.GetValues(typeof(Face))) {
                Chunk newChunk = chunk.chunkManager.getChunk(chunk.position + (FaceOffset.getOffsetOfFace(face) * 16));
                newChunk.setMinimumWantedChunkState(chunkState);
                chunk.chunksNeighbors[(int)face] = newChunk;
                if(newChunk is null) throw new Exception("fail to init neighboor chunk");
            }
        }
    }

    public virtual void Dispose(){}

    public virtual void update(double deltaTime) { }

    protected virtual Vector3D<float> ChunkStrategyColor() => new Vector3D<float>(1, 0, 0);

    public void debug(bool? setDebug = null) {
        chunk.debugMode = setDebug ?? !chunk.debugMode;
        
        if (!chunk.debugMode) {
            chunk.debugRay?.Dispose();
            chunk.debugRay = null;
        }
        else {
            if(chunk.debugRay != null) chunk.debugRay.Dispose();
            Vector3D<float> color = ChunkStrategyColor();
            LineVertex[] vertices = new[]
            {
                //base
                new LineVertex(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f), color),
                new LineVertex(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f , chunk.position.Y - 0.5f, chunk.position.Z - 0.5f), color),
                new LineVertex(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), color),
                new LineVertex(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), color),
                new LineVertex(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f), color),
                
                //top base
                new LineVertex(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f), color),
                new LineVertex(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f , chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f), color),
                new LineVertex(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), color),
                new LineVertex(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), color),
                new LineVertex(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f), color),
                
                //between
                new LineVertex(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), color),
            };
            chunk.debugRay = new Line(vertices, LineType.STRIP);
        }
    }
    
    public virtual void init(){}
    
    


    public virtual ChunkState minimumChunkStateOfNeighbors() => ChunkState.EMPTY;

    public virtual ReadOnlySpan<CubeVertex> getVertices() {
        throw new Exception("not availabe for this chunk state : " + chunk.chunkState.ToString());
    }
}