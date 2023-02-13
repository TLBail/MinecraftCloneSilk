using MinecraftCloneSilk.Core;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkBlockGeneratedStrategy : ChunkStrategy
{
    private const string PATHTOWORLDSAVE = "./Worlds/newWorld";
    
    public override ChunkState getChunkStateOfStrategy() => ChunkState.BLOCKGENERATED;
    public override void setBlock(int x, int y, int z, string name) {
        lock (chunk.blocksLock) {
            chunk.blocks[x, y, z].id = name.GetHashCode();
        }
    }

    public ChunkBlockGeneratedStrategy(Chunk chunk) : base(chunk) {    }

    public override void init() {
        
        //check if we have chunk in memory
        //if yes load it
        // if not check if chunk is in generated terrain and generate if not
        if (isChunkExistInMemory()) {
            Console.WriteLine("exist " + chunk.position);
            loadBlocks();
        }
        
        if (chunk.chunkState != ChunkState.Generatedterrain) {
            chunk.chunkStrategy = new ChunkTerrainGeneratedStrategy(chunk);
            chunk.chunkStrategy.init();
            chunk.chunkStrategy = this;
        }
        updateNeighboorChunkState(ChunkState.Generatedterrain);
        chunk.chunkState = ChunkState.BLOCKGENERATED;
    }

    private string pathToChunk() =>  PATHTOWORLDSAVE + "/" + chunk.position.X + "  " + chunk.position.Y  + "  " + chunk.position.Z;
    private bool isChunkExistInMemory() {
        return Directory.Exists(PATHTOWORLDSAVE) && File.Exists(pathToChunk());
    }
    
     public override void debug(bool? setDebug = null) {
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
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f , chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));

            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y  - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE- 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));
            
            //top base

            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X - 0.5f , chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f , chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));

            
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y +Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));
            
            //between
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X  - 0.5f, chunk.position.Y +  Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y +  Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z- 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z + Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y +  Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z+ Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z + Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(chunk.position.X - 0.5f , chunk.position.Y +  Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z+ Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));
            
            // diagonal
            chunk.debugRays.Add(new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y -0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f , chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f , chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f)));

        }
    }

    
    private void loadBlocks() {
        
    }

    
}