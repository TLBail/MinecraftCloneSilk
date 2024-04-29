using System.Diagnostics;
using System.Text;
using MinecraftCloneSilk.Model.NChunk;

namespace MinecraftCloneSilk.Model;

[Flags]
public enum ChunkFace
{
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


    public static ChunkFace GetChunkFaceFlags(BlockFactory blockFactory, IChunkData chunkData) {
        return GetChunkFaceFlags(blockFactory, chunkData.GetBlocks());
    }

    public static string toString(ChunkFace chunkFace) {
        //get all the flags of the chunkFace
        StringBuilder stringBuilder = new StringBuilder();
        foreach (ChunkFace chf in Enum.GetValues(typeof(ChunkFace))) {
            if ((chunkFace & chf) == chf) {
                stringBuilder.Append(chf.ToString());
                stringBuilder.Append(" ");
            }
        }
        return stringBuilder.ToString();
    }
    
    
    public static ChunkFace GetChunkFaceFlags(BlockFactory blockFactory, BlockData[,,] blocks) {
        ChunkFace faceFlag = ChunkFaceUtils.ALL;

        Span<BlockData> blocksSpan = ChunkData.GetSpan(blocks);

        for (int i = 0; i < blocksSpan.Length; i++) {
            if (blocksSpan[i].id != 0) {
                faceFlag &= ~ChunkFace.EMPTYCHUNK;
                break;
            }
        }
        
        // Check each face
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++) {
                // Top
                BlockData block = blocks[i, Chunk.CHUNK_SIZE - 1, j];
                if (block.id != 0) {
                    if (!blockFactory.IsBlockTransparent(block)) {
                        faceFlag &= ~ChunkFace.TOPTRANSPARENT;
                    } else {
                        faceFlag &= ~ChunkFace.TOPOPAQUE;
                    }
                }else {
                    faceFlag &= ~ChunkFace.TOPOPAQUE;
                }

                // Bottom
                block = blocks[i, 0, j];
                if (block.id != 0) {
                    if (!blockFactory.IsBlockTransparent(block)) {
                        faceFlag &= ~ChunkFace.BOTTOMTRANSPARENT;
                    }else{
                        faceFlag &= ~ChunkFace.BOTTOMOPAQUE;
                    }
                }else{
                    faceFlag &= ~ChunkFace.BOTTOMOPAQUE;
                }

                // Left
                block = blocks[0, i, j];
                if (block.id != 0) {
                    if (!blockFactory.IsBlockTransparent(block)) {
                        faceFlag &= ~ChunkFace.LEFTTRANSPARENT;
                    }else{
                        faceFlag &= ~ChunkFace.LEFTOPAQUE;
                    }
                }else{
                    faceFlag &= ~ChunkFace.LEFTOPAQUE;
                }

                // Right
                block = blocks[Chunk.CHUNK_SIZE - 1, i, j];
                if (block.id != 0) {
                    if (!blockFactory.IsBlockTransparent(block)) {
                        faceFlag &= ~ChunkFace.RIGHTTRANSPARENT;
                    }else{
                        faceFlag &= ~ChunkFace.RIGHTOPAQUE;
                    }
                }else{
                    faceFlag &= ~ChunkFace.RIGHTOPAQUE;
                }

                // Front
                block = blocks[i, j, 0];
                if (block.id != 0) {
                    if (!blockFactory.IsBlockTransparent(block)) {
                        faceFlag &= ~ChunkFace.FRONTTRANSPARENT;
                    }else{
                        faceFlag &= ~ChunkFace.FRONTOPAQUE;
                    }
                }else{
                    faceFlag &= ~ChunkFace.FRONTOPAQUE;
                }

                // Back
                block = blocks[i, j, Chunk.CHUNK_SIZE - 1];
                if (block.id != 0) {
                    if (!blockFactory.IsBlockTransparent(block)) {
                        faceFlag &= ~ChunkFace.BACKTRANSPARENT;
                    }else{
                        faceFlag &= ~ChunkFace.BACKOPAQUE;
                    }
                }else{
                    faceFlag &= ~ChunkFace.BACKOPAQUE;
                }
            }
        }

        return faceFlag;
    }

    public static void OnBlockSet(ref ChunkFace? chunkFace, BlockData oldBlockData, BlockData newBlockData, int x, int y, int z) {
        if (oldBlockData.id == newBlockData.id) return;
        if (oldBlockData.id == 0 && newBlockData.id != 0) chunkFace &= ~ChunkFace.EMPTYCHUNK;
        bool oldBlockTransparent = Chunk.blockFactory!.IsBlockTransparent(oldBlockData);
        bool newBlockTransparent = Chunk.blockFactory!.IsBlockTransparent(newBlockData);
        if(oldBlockTransparent == newBlockTransparent) return;
        
        if (x == 0) {
            if (newBlockTransparent) {
                chunkFace &= ~ChunkFace.LEFTOPAQUE;
            } else {
                chunkFace &= ~ChunkFace.LEFTTRANSPARENT;
            }
        } else if (x == Chunk.CHUNK_SIZE - 1) {
            if (newBlockTransparent) {
                chunkFace &= ~ChunkFace.RIGHTOPAQUE;
            } else {
                chunkFace &= ~ChunkFace.RIGHTTRANSPARENT;
            }
        }
        
        if (y == 0) {
            if (newBlockTransparent) {
                chunkFace &= ~ChunkFace.BOTTOMOPAQUE;
            } else {
                chunkFace &= ~ChunkFace.BOTTOMTRANSPARENT;
            }
        } else if (y == Chunk.CHUNK_SIZE - 1) {
            if (newBlockTransparent) {
                chunkFace &= ~ChunkFace.TOPOPAQUE;
            } else {
                chunkFace &= ~ChunkFace.TOPTRANSPARENT;
            }
        }
        
        if (z == 0) {
            if (newBlockTransparent) {
                chunkFace &= ~ChunkFace.FRONTOPAQUE;
            } else {
                chunkFace &= ~ChunkFace.FRONTTRANSPARENT;
            }
        } else if (z == Chunk.CHUNK_SIZE - 1) {
            if (newBlockTransparent) {
                chunkFace &= ~ChunkFace.BACKOPAQUE;
            } else {
                chunkFace &= ~ChunkFace.BACKTRANSPARENT;
            }
        }
        
    }

    private const ChunkFace ALL = ChunkFace.TOPOPAQUE | ChunkFace.TOPTRANSPARENT |
                                  ChunkFace.BOTTOMOPAQUE | ChunkFace.BOTTOMTRANSPARENT |
                                  ChunkFace.LEFTOPAQUE | ChunkFace.LEFTTRANSPARENT |
                                  ChunkFace.RIGHTOPAQUE | ChunkFace.RIGHTTRANSPARENT |
                                  ChunkFace.FRONTOPAQUE | ChunkFace.FRONTTRANSPARENT | 
                                  ChunkFace.BACKOPAQUE | ChunkFace.BACKTRANSPARENT | ChunkFace.EMPTYCHUNK
        ;
}