﻿using System.IO.Compression;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model.ChunkManagement;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.Model.NChunk;

public abstract class ChunkStrategy
{
    
    protected readonly Chunk chunk;
    protected ChunkStrategy(Chunk chunk) {
        this.chunk = chunk;
    }
    public abstract ChunkState GetChunkStateOfStrategy();
    public virtual ChunkState MinimumChunkStateOfNeighbors() => ChunkState.EMPTY;

    public virtual BlockData GetBlockData(Vector3D<int> position) {
        FaceFlag faceFlag = FaceFlag.EMPTY;
        if (position.Y < 0) {
            faceFlag |= FaceFlag.BOTTOM;
            position.Y += (int)Chunk.CHUNK_SIZE;
        } else if (position.Y >= Chunk.CHUNK_SIZE) {
            faceFlag |= FaceFlag.TOP;
            position.Y -= (int)Chunk.CHUNK_SIZE;
        }
        if (position.X < 0) {
            faceFlag |= FaceFlag.LEFT;
            position.X += (int)Chunk.CHUNK_SIZE;
        } else if (position.X >= Chunk.CHUNK_SIZE) {
            faceFlag |= FaceFlag.RIGHT;
            position.X -= (int)Chunk.CHUNK_SIZE;
        }
        if (position.Z < 0) {
            faceFlag |= FaceFlag.BACK;
            position.Z += (int)Chunk.CHUNK_SIZE;
        } else if (position.Z >= Chunk.CHUNK_SIZE) {
            faceFlag |= FaceFlag.FRONT;
            position.Z -= (int)Chunk.CHUNK_SIZE;
        }

        FaceExtended? faceExtended = FaceFlagUtils.GetFaceExtended(faceFlag);
        if (faceExtended is not null) {
            return chunk.chunksNeighbors![(int)faceExtended].GetBlockData(position);
        } else {
            return chunk.chunkData.GetBlock(position.X, position.Y, position.Z);
        }
    }
    public void SetBlockData(int x, int y, int z, BlockData blockData) {
        if(chunk.chunksNeighbors is null) throw new Exception("chunk neighbors not setup");
        FaceFlag faceFlag = FaceFlag.EMPTY;
        if (y < 0) {
            faceFlag |= FaceFlag.BOTTOM;
            y += (int)Chunk.CHUNK_SIZE;
        } else if (y >= Chunk.CHUNK_SIZE) {
            faceFlag |= FaceFlag.TOP;
            y -= (int)Chunk.CHUNK_SIZE;
        }
        if (x < 0) {
            faceFlag |= FaceFlag.LEFT;
            x += (int)Chunk.CHUNK_SIZE;
        } else if (x >= Chunk.CHUNK_SIZE) {
            faceFlag |= FaceFlag.RIGHT;
            x -= (int)Chunk.CHUNK_SIZE;
        }
        if (z < 0) {
            faceFlag |= FaceFlag.BACK;
            z += (int)Chunk.CHUNK_SIZE;
        } else if (z >= Chunk.CHUNK_SIZE) {
            faceFlag |= FaceFlag.FRONT;
            z -= (int)Chunk.CHUNK_SIZE;
        }

        FaceExtended? faceExtended = FaceFlagUtils.GetFaceExtended(faceFlag);
        if (faceExtended is not null) {
            chunk.chunksNeighbors![(int)faceExtended].chunkData.SetBlock(blockData, x, y, z);
        } else {
            chunk.chunkData.SetBlock(blockData,x, y, z);
        }
    }

    public virtual void UpdateChunkVertex() {
    }

    public virtual void Draw(GL gl, double deltaTime) {
    }

    public virtual void SetBlock(int x, int y, int z, string name) {
        chunk.chunkData.SetBlock(Chunk.blockFactory!.GetBlockData(name),x, y, z);
        UpdateChunkFaces();
        OnBlockSet(x, y, z);
    }

    public virtual void OnBlockSet(int x, int y, int z){}

    public virtual Block GetBlock(int x, int y, int z) {
        var blockData = GetBlockData(new Vector3D<int>(x, y, z));
        return Chunk.blockFactory!.BuildFromBlockData(new Vector3D<int>(x, y, z), blockData);
    }

    public virtual void Update(double deltaTime) {
    }

    protected virtual Vector3D<float> ChunkStrategyColor() => new Vector3D<float>(1, 0, 0);

    public void Debug(bool? setDebug = null) {
        chunk.debugMode = setDebug ?? !chunk.debugMode;

        if (!chunk.debugMode) {
            chunk.debugRay?.Dispose();
            chunk.debugRay = null;
        } else {
            if (chunk.debugRay != null) chunk.debugRay.Dispose();
            Vector3D<float> color = ChunkStrategyColor();
            LineVertex[] vertices = new[]
            {
                //base
                new LineVertex(
                    new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                    color),
                new LineVertex(
                    new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f,
                        chunk.position.Z - 0.5f), color),
                new LineVertex(
                    new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f,
                        chunk.position.Z + Chunk.CHUNK_SIZE - 0.5f), color),
                new LineVertex(
                    new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f,
                        chunk.position.Z + Chunk.CHUNK_SIZE - 0.5f), color),
                new LineVertex(
                    new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                    color),

                //top base
                new LineVertex(
                    new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f,
                        chunk.position.Z - 0.5f), color),
                new LineVertex(
                    new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f,
                        chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f), color),
                new LineVertex(
                    new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f,
                        chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z + Chunk.CHUNK_SIZE - 0.5f), color),
                new LineVertex(
                    new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f,
                        chunk.position.Z + Chunk.CHUNK_SIZE - 0.5f), color),
                new LineVertex(
                    new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f,
                        chunk.position.Z - 0.5f), color),

                //between
                new LineVertex(
                    new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f,
                        chunk.position.Z + Chunk.CHUNK_SIZE - 0.5f), color),
            };
            chunk.debugRay = new Line(vertices, LineType.STRIP);
        }
    }


    public void UpdateChunkFaces() {
        chunk.chunkFace = ChunkFaceUtils.GetChunkFaceFlags(Chunk.blockFactory!, chunk.chunkData);
    }

    public virtual void Init() {
    }

    public virtual ReadOnlySpan<CubeVertex> GetVertices() {
        throw new Exception("not availabe for this chunk state : " + chunk.chunkState.ToString());
    }

    public virtual void Load() { }

    public virtual void Finish() {}
}