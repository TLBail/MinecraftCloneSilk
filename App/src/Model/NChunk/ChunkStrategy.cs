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
        System.Diagnostics.Debug.Assert(position.X >= 0 && position.X < Chunk.CHUNK_SIZE &&
                                        position.Y >= 0 && position.Y < Chunk.CHUNK_SIZE &&
                                        position.Z >= 0 && position.Z < Chunk.CHUNK_SIZE, "position must be in the chunk");

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
            Chunk neighbor = chunk.chunksNeighbors![(int)faceExtended];
            BlockData oldBlockData = neighbor.chunkData.GetBlock(x, y, z);
            neighbor.chunkData.SetBlock(blockData, x, y, z);
            if (!oldBlockData.Equals(blockData)) {
                neighbor.blockModified = true;
                ChunkFaceUtils.OnBlockSet(ref neighbor.chunkFace, oldBlockData, blockData, x, y, z);
            }
        } else {
            chunk.chunkData.SetBlock(blockData,x, y, z);
            chunk.blockModified = true;
        }
    }


    public virtual void UpdateChunkVertex() { } // happen a lot => menfou

    public virtual void Draw(GL gl, double deltaTime) => throw new Exception("try to draw a chunk that is not ready to be drawn"); 

    public virtual void SetBlock(int x, int y, int z, string name) {
        BlockData oldBlockData = chunk.chunkData.GetBlock(x,y,z);
        BlockData newBlockData = Chunk.blockFactory!.GetBlockData(name);
        chunk.chunkData.SetBlock(newBlockData,x, y, z);
        ChunkFaceUtils.OnBlockSet(ref chunk.chunkFace, oldBlockData, newBlockData, x, y, z);
        OnBlockSet(x, y, z, oldBlockData, newBlockData);
    }

    public virtual void OnBlockSet(int x, int y, int z, BlockData oldBlockData, BlockData newBlockData){}

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
    
    protected void SetupNeighbors() {
        chunk.chunksNeighbors = new Chunk[26];
        foreach (FaceExtended face in FaceExtendedConst.FACES) {
            Vector3D<int> position = chunk.position + (FaceExtendedOffset.GetOffsetOfFace(face) * Chunk.CHUNK_SIZE);
            System.Diagnostics.Debug.Assert(chunk.chunkManager.ContainChunk(position),
                "chunk must be already generated");
            Chunk newChunk = chunk.chunkManager.GetChunk(position);
            System.Diagnostics.Debug.Assert(
                newChunk.chunkState >= MinimumChunkStateOfNeighbors(),
                " chunk must be at least at the same state as the minimum chunk state of neighborsh"
            );
            chunk.chunksNeighbors[(int)face] = newChunk;
        }
    }


    public virtual void Load() { }

    public virtual void Finish() {}
}