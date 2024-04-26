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
        int idSand = Chunk.blockFactory!.GetBlockIdByName("sand");
        BlockData[,,] blocks = chunk.chunkData.GetBlocks();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    if ((blocks[x, y, z].id == idGrass ||
                         blocks[x,y,z].id == idSand)&&
                        chunk.worldGenerator.HaveTreeOnThisCoord(chunk.position.X + x,chunk.position.Y + y, chunk.position.Z + z)) {
                        addTreeOnThisBlock(x, y, z);
                        
                    }
                }
            }
        }
    }

   

    private void addTreeOnThisBlock(int x, int y, int z) {
        StructureBlock[] structure;
        if (chunk.worldGenerator.IsDesert(chunk.position.X + x,chunk.position.Y + y, chunk.position.Z + z)) {
            structure = palmTreeStructure;
        } else {
            structure = treeStructure;
        }
        
        foreach (var strucutreBlock in structure) {
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
    
    private static StructureBlock[] palmTreeStructure = new StructureBlock[]
    {
        // Tronc
        new(0, 1, 0, 6),
        new(0, 2, 0, 6),
        new(0, 3, 0, 6),
        new(0, 4, 0, 6),
        new(0, 5, 0, 6),
        new(0, 6, 0, 6),
        new(0, 7, 0, 6),
        new(0, 8, 0, 6),
        new(0, 9, 0, 6),
        
        //top foliage
        new (0,10,0, 18),
        
        // forward foliage
        new (0,10, 1, 18),
        new (0,9, 1, 18),
        new (0,9, 2, 18),
        new (0,9, 3, 18),
        new (0,8, 4, 18),
        new (0,7, 4, 18),
        
        // Backward foliage (mirror of forward)
        new StructureBlock(0, 10, -1, 18),
        new StructureBlock(0, 9, -1, 18),
        new StructureBlock(0, 9, -2, 18),
        new StructureBlock(0, 9, -3, 18),
        new StructureBlock(0, 8, -4, 18),
        new StructureBlock(0, 7, -4, 18),

        // Left foliage (rotate forward 90 degrees counterclockwise)
        new StructureBlock(-1, 10, 0, 18),
        new StructureBlock(-1, 9, 0, 18),
        new StructureBlock(-2, 9, 0, 18),
        new StructureBlock(-3, 9, 0, 18),
        new StructureBlock(-4, 8, 0, 18),
        new StructureBlock(-4, 7, 0, 18),

        // Right foliage (rotate forward 90 degrees clockwise)
        new StructureBlock(1, 10, 0, 18),
        new StructureBlock(1, 9, 0, 18),
        new StructureBlock(2, 9, 0, 18),
        new StructureBlock(3, 9, 0, 18),
        new StructureBlock(4, 8, 0, 18),
        new StructureBlock(4, 7, 0, 18),
    };
}