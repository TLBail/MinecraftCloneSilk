using System.IO.Compression;
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

    public virtual BlockData GetBlockData(Vector3D<int> localPosition) {
        if (localPosition.Y < 0) {
            return chunk.chunksNeighbors![(int)Face.BOTTOM]!
                .GetBlockData(new Vector3D<int>(localPosition.X, localPosition.Y + (int)Chunk.CHUNK_SIZE,
                    localPosition.Z));
        } else if (localPosition.Y >= Chunk.CHUNK_SIZE) {
            return chunk.chunksNeighbors![(int)Face.TOP]!
                .GetBlockData(new Vector3D<int>(localPosition.X, localPosition.Y - (int)Chunk.CHUNK_SIZE,
                    localPosition.Z));
        } else if (localPosition.X < 0) {
            return chunk.chunksNeighbors![(int)Face.LEFT]!
                .GetBlockData(new Vector3D<int>(localPosition.X + (int)Chunk.CHUNK_SIZE, localPosition.Y,
                    localPosition.Z));
        } else if (localPosition.X >= Chunk.CHUNK_SIZE) {
            return chunk.chunksNeighbors![(int)Face.RIGHT]!
                .GetBlockData(new Vector3D<int>(localPosition.X - (int)Chunk.CHUNK_SIZE, localPosition.Y,
                    localPosition.Z));
        } else if (localPosition.Z < 0) {
            return chunk.chunksNeighbors![(int)Face.BACK]!
                .GetBlockData(new Vector3D<int>(localPosition.X, localPosition.Y,
                    localPosition.Z + (int)Chunk.CHUNK_SIZE));
        } else if (localPosition.Z >= Chunk.CHUNK_SIZE) {
            return chunk.chunksNeighbors![(int)Face.FRONT]!
                .GetBlockData(new Vector3D<int>(localPosition.X, localPosition.Y,
                    localPosition.Z - (int)Chunk.CHUNK_SIZE));
        } else {
            return chunk.blocks[localPosition.X, localPosition.Y, localPosition.Z];
        }
    }


    public virtual void UpdateChunkVertex() {
    }

    public virtual void Draw(GL gl, double deltaTime) {
    }

    public abstract void SetBlock(int x, int y, int z, string name);

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

    public virtual void Init() {
    }

    public virtual ReadOnlySpan<CubeVertex> GetVertices() {
        throw new Exception("not availabe for this chunk state : " + chunk.chunkState.ToString());
    }

    public virtual void Load() { }

    public virtual void Finish() {}
}