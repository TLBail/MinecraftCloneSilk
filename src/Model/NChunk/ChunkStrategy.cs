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
    protected void updateNeighboorChunkState(ChunkState chunkState) {
        lock (chunk.chunksNeighborsLock) {
            foreach (Face face in Enum.GetValues(typeof(Face))) {
                Chunk newChunk = chunk.chunkManager.getChunk(chunk.position + (FaceOffset.getOffsetOfFace(face) * 16));
                newChunk.setMinimumWantedChunkState(chunkState);
                chunk.chunksNeighbors[(int)face] = newChunk;
            }   
        }
    }

    public virtual void Dispose(){}
    
    public virtual void update(double deltaTime){}

    public virtual void debug(bool? setDebug = null) {
        chunk.debugMode = setDebug ?? !chunk.debugMode;

        
        if (!chunk.debugMode) {
            foreach (var debugRay in chunk.debugRays) {
                debugRay.remove();
            }
            chunk.debugRays.Clear();
        }
        else {
            //base
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y -0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f , chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f)));

            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y  - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE- 0.5f)));
            
            //top base

            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X - 0.5f , chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f , chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f)));

            
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y +Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f)));
            
            //between
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X  - 0.5f, chunk.position.Y +  Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y +  Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z- 0.5f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z + Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y +  Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z+ Chunk.CHUNK_SIZE - 0.5f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z + Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(chunk.position.X - 0.5f , chunk.position.Y +  Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z+ Chunk.CHUNK_SIZE - 0.5f)));
        }

    }
    
    public virtual void init(){}


    public virtual ChunkState minimumChunkStateOfNeighbors() => ChunkState.EMPTY;
}