using MinecraftCloneSilk.Model.Lighting;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkLightingStrategy : ChunkStrategy
{
    public ChunkLightingStrategy(Chunk chunk) : base(chunk) {
        if (chunk.chunkState != ChunkState.BLOCKGENERATED) {
            throw new Exception("failed to init chunkDrawableStrategy because the chunk is not BLOCKGENERATED");
        }
    }
    public override ChunkState MinimumChunkStateOfNeighbors() => ChunkState.BLOCKGENERATED;
    public override ChunkState GetChunkStateOfStrategy() => ChunkState.LIGHTING;
    


    public override void Init() {
        chunk.chunkState = ChunkState.LIGHTLOADING;
        System.Diagnostics.Debug.Assert(chunk.chunkFace is not null);
        SetupNeighbors();
    }
    protected virtual void SetupNeighbors() {
        chunk.chunksNeighbors = new Chunk[26];
        foreach (FaceExtended face in FaceExtendedConst.FACES) {
            Vector3D<int> positionNeibor = chunk.position + (FaceExtendedOffset.GetOffsetOfFace(face) * Chunk.CHUNK_SIZE);
            System.Diagnostics.Debug.Assert(chunk.chunkManager.ContainChunk(positionNeibor),
                "chunk must be already generated");
            Chunk newChunk = chunk.chunkManager.GetChunk(positionNeibor);
            System.Diagnostics.Debug.Assert(newChunk.chunkState >= MinimumChunkStateOfNeighbors(), "try to setup a chunk with a lower state than the minimum"); 
            chunk.chunksNeighbors[(int)face] = newChunk;
        }
    }

    protected override Vector3D<float> ChunkStrategyColor() => new Vector3D<float>(0.5f, 0.5f, 0.5f);

    public override void Load() {
        // chunk.chunkLightManager.FullLightChunk(chunk).Wait();
        LightCalculator.LightChunk(chunk);
    }



    public override void Finish() {
        chunk.chunkState = ChunkState.LIGHTING;
    }
}