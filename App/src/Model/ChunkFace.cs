using MinecraftCloneSilk.Model.NChunk;

namespace MinecraftCloneSilk.Model;

[Flags]
public enum ChunkFace
{
	EMPTY = 0,
	TOPOPAQUE = 1,
	TOPTRANSPARENT = 2,
	BOTTOMOPAQUE = 4,
	BOTTOMTRANSPARENT = 8,
	LEFTOPAQUE = 16,
	LEFTTRANSPARENT = 32,
	RIGHTOPAQUE = 64,
	RIGHTTRANSPARENT = 128,
	FRONTOPAQUE = 256,
	FRONTTRANSPARENT = 512,
	BACKOPAQUE = 1024,
	BACKTRANSPARENT = 2048,
	EMPTYCHUNK = 4096
}


public static class ChunkFaceUtils
{
	public const ChunkFace ALLTRANSPARENT = ChunkFace.TOPTRANSPARENT | ChunkFace.BOTTOMTRANSPARENT | ChunkFace.LEFTTRANSPARENT | ChunkFace.RIGHTTRANSPARENT | ChunkFace.FRONTTRANSPARENT | ChunkFace.BACKTRANSPARENT;
	public const ChunkFace ALLOPAQUE = ChunkFace.TOPOPAQUE | ChunkFace.BOTTOMOPAQUE | ChunkFace.LEFTOPAQUE | ChunkFace.RIGHTOPAQUE | ChunkFace.FRONTOPAQUE | ChunkFace.BACKOPAQUE;
	
	public static bool IsOpaque(ChunkFace face) {
		return (face & ALLOPAQUE) == ALLOPAQUE;
	}
	
	public static bool IsTransparent(ChunkFace face) {
		return (face & ALLTRANSPARENT) == ALLTRANSPARENT;
	}


    public static ChunkFace GetChunkFaceFlags(BlockFactory blockFactory, ChunkData chunkData) {
        if(chunkData.IsOnlyOneBlock() ) {
            if (chunkData.GetBlock().id == 0) {
                return ChunkFace.EMPTYCHUNK;
            }
        }
        return GetChunkFaceFlags(blockFactory, chunkData.GetBlocks());
    }
    public static ChunkFace GetChunkFaceFlags(BlockFactory blockFactory, BlockData[,,] blocks) {
        ChunkFace faceFlag = ChunkFace.EMPTYCHUNK |
                             ChunkFace.TOPOPAQUE | ChunkFace.TOPTRANSPARENT |
                             ChunkFace.BOTTOMOPAQUE | ChunkFace.BOTTOMTRANSPARENT |
                             ChunkFace.LEFTOPAQUE | ChunkFace.LEFTTRANSPARENT |
                             ChunkFace.RIGHTOPAQUE | ChunkFace.RIGHTTRANSPARENT |
                             ChunkFace.FRONTOPAQUE | ChunkFace.FRONTTRANSPARENT | 
                             ChunkFace.BACKOPAQUE | ChunkFace.BACKTRANSPARENT
            ;
		
        // Verify if the chunk is empty or not
        bool isEmpty = true;
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    if (blocks[x, y, z].id != 0) {
                        isEmpty = false;
                        break;
                    }
                }
                if (!isEmpty) break;
            }
            if (!isEmpty) break;
        }

        if (isEmpty) {
            return faceFlag;
        } else {
            faceFlag ^= ChunkFace.EMPTYCHUNK;
        }

        // Check each face
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++) {
                // Top
                BlockData block = blocks[i, Chunk.CHUNK_SIZE - 1, j];
                if (block.id != 0) {
                    if (blockFactory.IsBlockTransparent(block)) {
                        if ((faceFlag & ChunkFace.TOPOPAQUE) > 0) {
                            faceFlag ^= ChunkFace.TOPOPAQUE;
                        }
                    } else {
                        if ((faceFlag & ChunkFace.TOPTRANSPARENT) > 0) {
                            faceFlag ^= ChunkFace.TOPTRANSPARENT;
                        }
                    }
                }

                // Bottom
                block = blocks[i, 0, j];
                if (block.id != 0) {
                    if (blockFactory.IsBlockTransparent(block)) {
                        if ((faceFlag & ChunkFace.BOTTOMOPAQUE) > 0) {
                            faceFlag ^= ChunkFace.BOTTOMOPAQUE;
                        }
                    } else {
                        if ((faceFlag & ChunkFace.BOTTOMTRANSPARENT) > 0) {
                            faceFlag ^= ChunkFace.BOTTOMTRANSPARENT;
                        }
                    }
                }

                // Left
                block = blocks[0, i, j];
                if (block.id != 0) {
                    if (blockFactory.IsBlockTransparent(block)) {
                        if ((faceFlag & ChunkFace.LEFTOPAQUE) > 0) {
                            faceFlag ^= ChunkFace.LEFTOPAQUE;
                        }
                    } else {
                        if ((faceFlag & ChunkFace.LEFTTRANSPARENT) > 0) {
                            faceFlag ^= ChunkFace.LEFTTRANSPARENT;
                        }
                    }
                }

                // Right
                block = blocks[Chunk.CHUNK_SIZE - 1, i, j];
                if (block.id != 0) {
                    if (blockFactory.IsBlockTransparent(block)) {
                        if ((faceFlag & ChunkFace.RIGHTOPAQUE) > 0) {
                            faceFlag ^= ChunkFace.RIGHTOPAQUE;
                        }
                    } else {
                        if ((faceFlag & ChunkFace.RIGHTTRANSPARENT) > 0) {
                            faceFlag ^= ChunkFace.RIGHTTRANSPARENT;
                        }
                    }
                }

                // Front
                block = blocks[i, j, 0];
                if (block.id != 0) {
                    if (blockFactory.IsBlockTransparent(block)) {
                        if ((faceFlag & ChunkFace.FRONTOPAQUE) > 0) {
                            faceFlag ^= ChunkFace.FRONTOPAQUE;
                        }
                    } else {
                        if ((faceFlag & ChunkFace.FRONTTRANSPARENT) > 0) {
                            faceFlag ^= ChunkFace.FRONTTRANSPARENT;
                        }
                    }
                }

                // Back
                block = blocks[i, j, Chunk.CHUNK_SIZE - 1];
                if (block.id != 0) {
                    if (blockFactory.IsBlockTransparent(block)) {
                        if ((faceFlag & ChunkFace.BACKOPAQUE) > 0) {
                            faceFlag ^= ChunkFace.BACKOPAQUE;
                        }
                    } else {
                        if ((faceFlag & ChunkFace.BACKTRANSPARENT) > 0) {
                            faceFlag ^= ChunkFace.BACKTRANSPARENT;
                        }
                    }
                }
            }
        }

        return faceFlag;
    }
}