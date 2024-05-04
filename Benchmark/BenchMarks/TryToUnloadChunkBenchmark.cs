using BenchmarkDotNet.Attributes;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;
using UnitTest.fakeClass;

namespace Benchmark.BenchMarks;

public class TryToUnloadChunkBenchmark
{
    
    private ChunkManagerEmpty chunkManagerEmpty = null!;
    private ChunkLoader chunkLoader = null!;
    private RegionStorage regionStorage = null!;
    
    [GlobalSetup]
    public void GlobalSetup() {
        Chunk.InitStaticMembers(null, BlockFactory.GetInstance());
        regionStorage = new RegionStorage("./Worlds/newWorld");
        chunkManagerEmpty = new ChunkManagerEmpty(new WorldFlatGeneration(), regionStorage);
        chunkLoader = new ChunkLoader(ChunkLoader.ChunkLoaderMode.SYNC);
        for (int i = 0; i < 16; i++) {
            for (int j = 0; j < 16; j++) {
                for (int k = 0; k < 16; k++) {
                    ChunkManagerTools.GetBlockGeneratedChunk(chunkManagerEmpty, chunkLoader, new Vector3D<int>(i * Chunk.CHUNK_SIZE,
                        j * Chunk.CHUNK_SIZE,
                        k * Chunk.CHUNK_SIZE));
                }
            }
        }
        
    }

    private ChunkState GetMinimumChunkStateOfChunk(Vector3D<int> position) {
        ChunkState chunkState = ChunkState.EMPTY;
        foreach (FaceExtended face in FaceExtendedConst.FACES) {
            if (chunkManagerEmpty.chunks.TryGetValue(position + (FaceExtendedOffset.GetOffsetOfFace(face) * Chunk.CHUNK_SIZE), out var chunk) &&
                chunk.GetMinimumChunkStateOfNeighbors() > chunkState) {
                chunkState = chunk.GetMinimumChunkStateOfNeighbors();
            }
        }
        return chunkState;
    }
    public bool TryToUnloadChunk(Vector3D<int> position) {
        Chunk chunkToUnload = chunkManagerEmpty.chunks[position];
        if(chunkToUnload.IsRequiredByChunkLoader()) return false;
        ChunkState minimumChunkStateOfChunk = GetMinimumChunkStateOfChunk(position);
        if (chunkToUnload.chunkState == ChunkState.DRAWABLE) {
            chunkToUnload.SetChunkState(ChunkState.BLOCKGENERATED);
            chunkToUnload.FinishChunkState();
        }
        if(minimumChunkStateOfChunk > ChunkState.EMPTY) return false;
        
        if (chunkManagerEmpty.chunks.Remove(position)) {
            return true;
        } else {
            return false;
        }
    }

    [Benchmark]
    public void TryToUnloadChunksBenchMark() {
        for (int i = 0; i < 16; i++) {
            for (int j = 0; j < 16; j++) {
                for (int k = 0; k < 16; k++) {
                    TryToUnloadChunk(new Vector3D<int>(i * Chunk.CHUNK_SIZE,
                        j * Chunk.CHUNK_SIZE,
                        k * Chunk.CHUNK_SIZE));
                }
            }
        }
        
    }
}