using System.IO.Hashing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using MinecraftCloneSilk.Core;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkBlockGeneratedStrategy : ChunkStrategy
{
    public override ChunkState GetChunkStateOfStrategy() => ChunkState.BLOCKGENERATED;

    private ChunkState minimumChunkStateOfNeighborsValue = ChunkState.EMPTY;
    public override ChunkState MinimumChunkStateOfNeighbors() => minimumChunkStateOfNeighborsValue;
    public ChunkBlockGeneratedStrategy(Chunk chunk) : base(chunk) { }

    public override void Init() {
        chunk.chunkState = ChunkState.BLOCKLOADING;
        minimumChunkStateOfNeighborsValue = ChunkState.GENERATEDTERRAIN;
        SetupNeighbors();
    }

    public override void Load() {
        GenerateStruture();
        chunk.blockModified = true;
    }

    public override void Finish() {
        UpdateChunkFaces();

#if DEBUG
        //verify block are well lighted
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    BlockData block = chunk.chunkData.GetBlock(x, y, z);
                    byte lightEmitting = Chunk.blockFactory!.blocks[block.id].lightEmitting;
                    byte lightLevelOfBlock = block.GetLightLevel();
                    System.Diagnostics.Debug.Assert(lightLevelOfBlock >= lightEmitting, $"block light level is not the same as the light emitting of the block {block} light emitting : {lightEmitting} light level : {block.GetLightLevel()}");
                }
            }
        }
        
#endif
        
        minimumChunkStateOfNeighborsValue = ChunkState.EMPTY;
        chunk.chunkState = ChunkState.BLOCKGENERATED;
    }


    private void SetupNeighbors() {
        chunk.chunksNeighbors = new Chunk[26];
        foreach (FaceExtended face in FaceExtendedConst.FACES) {
            Vector3D<int> position = chunk.position + (FaceExtendedOffset.GetOffsetOfFace(face) * Chunk.CHUNK_SIZE);
            System.Diagnostics.Debug.Assert(chunk.chunkManager.ContainChunk(position),
                "chunk must be already generated");
            Chunk newChunk = chunk.chunkManager.GetChunk(position);
            System.Diagnostics.Debug.Assert(
                newChunk.chunkState >= minimumChunkStateOfNeighborsValue,
                " chunk must be at least at the same state as the minimum chunk state of neighborsh"
                );
            chunk.chunksNeighbors[(int)face] = newChunk;
        }
    }


    protected override Vector3D<float> ChunkStrategyColor() => new Vector3D<float>(0, 0, 1);

    private void GenerateStruture() {
        int idGrass = Chunk.blockFactory!.GetBlockIdByName("grass");
        BlockData[,,] blocks = chunk.chunkData.GetBlocks();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    if (blocks[x, y, z].id == idGrass &&
                        chunk.worldGenerator.HaveTreeOnThisCoord(chunk.position.X + x, chunk.position.Z + z)) {
                        addTreeOnThisBlock(x, y, z);
                    }
                }
            }
        }
    }

   

    private void addTreeOnThisBlock(int x, int y, int z) {
        //Todo remove this check
        if (treeStructure == null) throw new InvalidOperationException("treeStructure is not initialized.");
        if (Chunk.blockFactory == null) throw new InvalidOperationException("blockFactory is not initialized.");

        
        foreach (var strucutreBlock in treeStructure) {
            SetBlockData(x + strucutreBlock.x, y + strucutreBlock.y, z + strucutreBlock.z,
                Chunk.blockFactory!.GetBlockData(strucutreBlock.id));
        }
    }

    private record struct StructureBlock(short x, short y, short z, short id);

    private static StructureBlock[] treeStructure = new StructureBlock[]
    {
        //tronc
        new StructureBlock(0, 1, 0, 12),
        new StructureBlock(0, 2, 0, 12),
        new StructureBlock(0, 3, 0, 12),
        new StructureBlock(0, 4, 0, 12),
        new StructureBlock(0, 5, 0, 12),
        //foliage
        //top
        new StructureBlock(0, 6, 0, 18),
        new StructureBlock(0, 6, 1, 18),
        new StructureBlock(0, 6, -1, 18),
        new StructureBlock(1, 6, 0, 18),
        new StructureBlock(-1, 6, 0, 18),
        //middle
        new StructureBlock(0, 5, 1, 18),
        new StructureBlock(0, 5, -1, 18),
        new StructureBlock(1, 5, 0, 18),
        new StructureBlock(-1, 5, 0, 18),
        new StructureBlock(1, 5, 1, 18),
        new StructureBlock(-1, 5, -1, 18),
        new StructureBlock(-1, 5, 1, 18),
        new StructureBlock(1, 5, -1, 18),

        //bottom
        new StructureBlock(0, 4, 1, 18),
        new StructureBlock(0, 4, -1, 18),
        new StructureBlock(1, 4, 0, 18),
        new StructureBlock(-1, 4, 0, 18),
        new StructureBlock(1, 4, 1, 18),
        new StructureBlock(-1, 4, -1, 18),
        new StructureBlock(-1, 4, 1, 18),
        new StructureBlock(1, 4, -1, 18),


        new StructureBlock(0, 4, 2, 18),
        new StructureBlock(0, 4, -2, 18),
        new StructureBlock(2, 4, 0, 18),
        new StructureBlock(-2, 4, 0, 18),
        new StructureBlock(2, 4, 1, 18),
        new StructureBlock(2, 4, -1, 18),
        new StructureBlock(-2, 4, 1, 18),
        new StructureBlock(-2, 4, -1, 18),
        new StructureBlock(1, 4, 2, 18),
        new StructureBlock(-1, 4, 2, 18),
        new StructureBlock(1, 4, -2, 18),
        new StructureBlock(-1, 4, -2, 18),
    };
}