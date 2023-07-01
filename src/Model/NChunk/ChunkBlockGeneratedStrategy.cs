using System.IO.Hashing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using MinecraftCloneSilk.Core;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkBlockGeneratedStrategy : ChunkStrategy
{
    public override ChunkState getChunkStateOfStrategy() => ChunkState.BLOCKGENERATED;

    private ChunkState minimumChunkStateOfNeighborsValue = ChunkState.EMPTY;
    public override ChunkState minimumChunkStateOfNeighbors() => minimumChunkStateOfNeighborsValue;

    public override void setBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = Chunk.blockFactory.getBlockIdByName(name);
    }

    public ChunkBlockGeneratedStrategy(Chunk chunk) : base(chunk) {
    }

    public override void init() {
        if (chunk.chunkState != ChunkState.GENERATEDTERRAIN) {
            throw new Exception("try to init a chunk with a wrong state");
        } 
        minimumChunkStateOfNeighborsValue = ChunkState.GENERATEDTERRAIN;
        setupNeighbors();
        generateStruture();
        minimumChunkStateOfNeighborsValue = ChunkState.EMPTY;
        chunk.blockModified = true;
        chunk.chunkState = ChunkState.BLOCKGENERATED;
    }

    private void setupNeighbors() {
        chunk.chunksNeighbors = new Chunk[26];
        foreach (FaceExtended face in Enum.GetValues(typeof(FaceExtended))) {
            Chunk newChunk =
                chunk.chunkManager.getChunk(chunk.position + (FaceExtendedOffset.getOffsetOfFace(face) * Chunk.CHUNK_SIZE));
            if(newChunk.chunkState < minimumChunkStateOfNeighborsValue) {
                throw new Exception("try to setup a chunk with a lower state than the minimum");
            }
            chunk.chunksNeighbors[(int)face] = newChunk;
        }
    }


    protected override Vector3D<float> ChunkStrategyColor() => new Vector3D<float>(0, 0, 1);

    private bool haveTreeOnThisCoord(int x, int z) => x % 20 == 0 && z % 20 == 0;

    private void generateStruture() {
        int idGrass = Chunk.blockFactory.getBlockIdByName("grass");
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    if (chunk.blocks[x, y, z].id == idGrass &&
                        haveTreeOnThisCoord(x, z)) {
                        addTreeOnThisBlock(x, y, z);
                    }
                }
            }
        }
    }

    protected virtual void setBlockData(int x, int y, int z, BlockData blockData) {
        if (y < 0) {
            if (x < 0) {
                if (z < 0) {
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMLEFTBACK].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMLEFTBACK].blocks[x + (int)Chunk.CHUNK_SIZE,
                        y + (int)Chunk.CHUNK_SIZE, z + (int)Chunk.CHUNK_SIZE] = blockData;
                } else if (z >= Chunk.CHUNK_SIZE) {
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMLEFTFRONT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMLEFTFRONT].blocks[x + (int)Chunk.CHUNK_SIZE,
                        y + (int)Chunk.CHUNK_SIZE, z - (int)Chunk.CHUNK_SIZE] = blockData;
                } else {
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMLEFT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMLEFT].blocks[x + (int)Chunk.CHUNK_SIZE,
                        y + (int)Chunk.CHUNK_SIZE, z] = blockData;
                }
            } else if (x >= Chunk.CHUNK_SIZE) {
                if (z < 0) {
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMRIGHTBACK].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMRIGHTBACK].blocks[x - (int)Chunk.CHUNK_SIZE,
                        y + (int)Chunk.CHUNK_SIZE, z + (int)Chunk.CHUNK_SIZE] = blockData;
                } else if (z >= Chunk.CHUNK_SIZE) {
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMRIGHTFRONT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMRIGHTFRONT].blocks[x - (int)Chunk.CHUNK_SIZE,
                        y + (int)Chunk.CHUNK_SIZE, z - (int)Chunk.CHUNK_SIZE] = blockData;
                } else {
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMRIGHT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMRIGHT].blocks[x - (int)Chunk.CHUNK_SIZE,
                        y + (int)Chunk.CHUNK_SIZE, z] = blockData;
                }
            } else {
                if (z < 0) {
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMBACK].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMBACK].blocks[x, y + (int)Chunk.CHUNK_SIZE,
                        z + (int)Chunk.CHUNK_SIZE] = blockData;
                } else if (z >= Chunk.CHUNK_SIZE) {
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMFRONT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOMFRONT].blocks[x, y + (int)Chunk.CHUNK_SIZE,
                        z - (int)Chunk.CHUNK_SIZE] = blockData;
                } else {
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOM].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.BOTTOM].blocks[x, y + (int)Chunk.CHUNK_SIZE, z] =
                        blockData;
                }
            }
        } else if (y >= Chunk.CHUNK_SIZE) {
            if (x < 0) {
                if (z < 0) {
                    chunk.chunksNeighbors[(int)FaceExtended.TOPLEFTBACK].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.TOPLEFTBACK].blocks[x + (int)Chunk.CHUNK_SIZE,
                        y - (int)Chunk.CHUNK_SIZE, z + (int)Chunk.CHUNK_SIZE] = blockData;
                } else if (z >= Chunk.CHUNK_SIZE) {
                    chunk.chunksNeighbors[(int)FaceExtended.TOPLEFTFRONT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.TOPLEFTFRONT].blocks[x + (int)Chunk.CHUNK_SIZE,
                        y - (int)Chunk.CHUNK_SIZE, z - (int)Chunk.CHUNK_SIZE] = blockData;
                } else {
                    chunk.chunksNeighbors[(int)FaceExtended.TOPLEFT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.TOPLEFT].blocks[x + (int)Chunk.CHUNK_SIZE,
                        y - (int)Chunk.CHUNK_SIZE, z] = blockData;
                }
            } else if (x >= Chunk.CHUNK_SIZE) {
                if (z < 0) {
                    chunk.chunksNeighbors[(int)FaceExtended.TOPRIGHTBACK].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.TOPRIGHTBACK].blocks[x - (int)Chunk.CHUNK_SIZE,
                        y - (int)Chunk.CHUNK_SIZE, z + (int)Chunk.CHUNK_SIZE] = blockData;
                } else if (z >= Chunk.CHUNK_SIZE) {
                    chunk.chunksNeighbors[(int)FaceExtended.TOPRIGHTFRONT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.TOPRIGHTFRONT].blocks[x - (int)Chunk.CHUNK_SIZE,
                        y - (int)Chunk.CHUNK_SIZE, z - (int)Chunk.CHUNK_SIZE] = blockData;
                } else {
                    chunk.chunksNeighbors[(int)FaceExtended.TOPRIGHT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.TOPRIGHT].blocks[x - (int)Chunk.CHUNK_SIZE,
                        y - (int)Chunk.CHUNK_SIZE, z] = blockData;
                }
            } else {
                if (z < 0) {
                    chunk.chunksNeighbors[(int)FaceExtended.TOPBACK].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.TOPBACK].blocks[x, y - (int)Chunk.CHUNK_SIZE,
                        z + (int)Chunk.CHUNK_SIZE] = blockData;
                } else if (z >= Chunk.CHUNK_SIZE) {
                    chunk.chunksNeighbors[(int)FaceExtended.TOPFRONT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.TOPFRONT].blocks[x, y - (int)Chunk.CHUNK_SIZE,
                        z - (int)Chunk.CHUNK_SIZE] = blockData;
                } else {
                    chunk.chunksNeighbors[(int)FaceExtended.TOP].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.TOP].blocks[x, y - (int)Chunk.CHUNK_SIZE, z] = blockData;
                }
            }
        } else {
            if (x < 0) {
                if (z < 0) {
                    chunk.chunksNeighbors[(int)FaceExtended.LEFTBACK].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.LEFTBACK].blocks[x + (int)Chunk.CHUNK_SIZE, y,
                        z + (int)Chunk.CHUNK_SIZE] = blockData;
                } else if (z >= Chunk.CHUNK_SIZE) {
                    chunk.chunksNeighbors[(int)FaceExtended.LEFTFRONT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.LEFTFRONT].blocks[x + (int)Chunk.CHUNK_SIZE, y,
                        z - (int)Chunk.CHUNK_SIZE] = blockData;
                } else {
                    chunk.chunksNeighbors[(int)FaceExtended.LEFT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.LEFT].blocks[x + (int)Chunk.CHUNK_SIZE, y, z] = blockData;
                }
            } else if (x >= Chunk.CHUNK_SIZE) {
                if (z < 0) {
                    chunk.chunksNeighbors[(int)FaceExtended.RIGHTBACK].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.RIGHTBACK].blocks[x - (int)Chunk.CHUNK_SIZE, y,
                        z + (int)Chunk.CHUNK_SIZE] = blockData;
                } else if (z >= Chunk.CHUNK_SIZE) {
                    chunk.chunksNeighbors[(int)FaceExtended.RIGHTFRONT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.RIGHTFRONT].blocks[x - (int)Chunk.CHUNK_SIZE, y,
                        z - (int)Chunk.CHUNK_SIZE] = blockData;
                } else {
                    chunk.chunksNeighbors[(int)FaceExtended.RIGHT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.RIGHT].blocks[x - (int)Chunk.CHUNK_SIZE, y, z] = blockData;
                }
            } else {
                if (z < 0) {
                    chunk.chunksNeighbors[(int)FaceExtended.BACK].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.BACK].blocks[x, y, z + (int)Chunk.CHUNK_SIZE] = blockData;
                } else if (z >= Chunk.CHUNK_SIZE) {
                    chunk.chunksNeighbors[(int)FaceExtended.FRONT].blockModified = true;
                    chunk.chunksNeighbors[(int)FaceExtended.FRONT].blocks[x, y, z - (int)Chunk.CHUNK_SIZE] = blockData;
                } else {
                    chunk.blocks[x, y, z] = blockData;
                }
            }
        }
    }

    private void addTreeOnThisBlock(int x, int y, int z) {
        foreach (var strucutreBlock in treeStructure) {
            setBlockData(x + strucutreBlock.x, y + strucutreBlock.y, z + strucutreBlock.z,
                Chunk.blockFactory.getBlockData(strucutreBlock.id));
        }
    }

    private record struct StructureBlock(short x, short y, short z, int id);

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

    static ChunkBlockGeneratedStrategy() {
        FaceExtended[] faces = (FaceExtended[])Enum.GetValues(typeof(FaceExtended));
        dependatesChunkOffset = new Vector3D<int>[faces.Length];
        for (int i = 0; i < faces.Length; i++) {
            dependatesChunkOffset[i] = FaceExtendedOffset.getOffsetOfFace(faces[i]) * Chunk.CHUNK_SIZE;
        }
    }

    public static readonly Vector3D<int>[] dependatesChunkOffset;

}