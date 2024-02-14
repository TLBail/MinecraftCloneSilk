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


    public static ChunkFace GetChunkFaceFlags(BlockFactory blockFactory, IChunkData lazyChunkData) {
        if(lazyChunkData.IsOnlyOneBlock() ) {
            if (lazyChunkData.GetBlock().id == 0) {
                return ChunkFace.EMPTYCHUNK;
            }
        }
        return GetChunkFaceFlags(blockFactory, lazyChunkData.GetBlocks());
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
        ChunkFace faceFlag = ChunkFace.TOPOPAQUE | ChunkFace.TOPTRANSPARENT |
                             ChunkFace.BOTTOMOPAQUE | ChunkFace.BOTTOMTRANSPARENT |
                             ChunkFace.LEFTOPAQUE | ChunkFace.LEFTTRANSPARENT |
                             ChunkFace.RIGHTOPAQUE | ChunkFace.RIGHTTRANSPARENT |
                             ChunkFace.FRONTOPAQUE | ChunkFace.FRONTTRANSPARENT | 
                             ChunkFace.BACKOPAQUE | ChunkFace.BACKTRANSPARENT
            ;
        // Check each face
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++) {
                // Top
                BlockData block = blocks[i, Chunk.CHUNK_SIZE - 1, j];
                if (block.id != 0) {
                    if (!blockFactory.IsBlockTransparent(block)) {
                        if((ChunkFace.TOPTRANSPARENT & faceFlag) != 0)
                            faceFlag ^= ChunkFace.TOPTRANSPARENT;
                    } else {
                        if((ChunkFace.TOPOPAQUE & faceFlag) != 0)
                            faceFlag ^= ChunkFace.TOPOPAQUE;
                    }
                }else {
                    if((ChunkFace.TOPOPAQUE & faceFlag) != 0)
                        faceFlag ^= ChunkFace.TOPOPAQUE;
                }

                // Bottom
                block = blocks[i, 0, j];
                if (block.id != 0) {
                    if (!blockFactory.IsBlockTransparent(block)) {
                        if((ChunkFace.BOTTOMTRANSPARENT & faceFlag) != 0)
                             faceFlag ^= ChunkFace.BOTTOMTRANSPARENT;
                    }else{
                        if((ChunkFace.BOTTOMOPAQUE & faceFlag) != 0)
                            faceFlag ^= ChunkFace.BOTTOMOPAQUE;
                    }
                }else{
                    if((ChunkFace.BOTTOMOPAQUE & faceFlag) != 0)
                        faceFlag ^= ChunkFace.BOTTOMOPAQUE;
                }

                // Left
                block = blocks[0, i, j];
                if (block.id != 0) {
                    if (!blockFactory.IsBlockTransparent(block)) {
                        if((ChunkFace.LEFTTRANSPARENT & faceFlag) != 0)
                             faceFlag ^= ChunkFace.LEFTTRANSPARENT;
                    }else{
                        if((ChunkFace.LEFTOPAQUE & faceFlag) != 0)
                            faceFlag ^= ChunkFace.LEFTOPAQUE;
                    }
                }else{
                    if((ChunkFace.LEFTOPAQUE & faceFlag) != 0)
                        faceFlag ^= ChunkFace.LEFTOPAQUE;
                }

                // Right
                block = blocks[Chunk.CHUNK_SIZE - 1, i, j];
                if (block.id != 0) {
                    if (!blockFactory.IsBlockTransparent(block)) {
                        if((ChunkFace.RIGHTTRANSPARENT & faceFlag) != 0)
                             faceFlag ^= ChunkFace.RIGHTTRANSPARENT;
                    }else{
                        if((ChunkFace.RIGHTOPAQUE & faceFlag) != 0)
                            faceFlag ^= ChunkFace.RIGHTOPAQUE;
                    }
                }else{
                    if((ChunkFace.RIGHTOPAQUE & faceFlag) != 0)
                        faceFlag ^= ChunkFace.RIGHTOPAQUE;
                }

                // Front
                block = blocks[i, j, 0];
                if (block.id != 0) {
                    if (!blockFactory.IsBlockTransparent(block)) {
                        if((ChunkFace.FRONTTRANSPARENT & faceFlag) != 0)
                             faceFlag ^= ChunkFace.FRONTTRANSPARENT;
                    }else{
                        if((ChunkFace.FRONTOPAQUE & faceFlag) != 0)
                            faceFlag ^= ChunkFace.FRONTOPAQUE;
                    }
                }else{
                    if((ChunkFace.FRONTOPAQUE & faceFlag) != 0)
                        faceFlag ^= ChunkFace.FRONTOPAQUE;
                }

                // Back
                block = blocks[i, j, Chunk.CHUNK_SIZE - 1];
                if (block.id != 0) {
                    if (!blockFactory.IsBlockTransparent(block)) {
                        if((ChunkFace.BACKTRANSPARENT & faceFlag) != 0)
                             faceFlag ^= ChunkFace.BACKTRANSPARENT;
                    }else{
                        if((ChunkFace.BACKOPAQUE & faceFlag) != 0)
                            faceFlag ^= ChunkFace.BACKOPAQUE;
                    }
                }else{
                    if((ChunkFace.BACKOPAQUE & faceFlag) != 0)
                        faceFlag ^= ChunkFace.BACKOPAQUE;
                }
            }
        }

        return faceFlag;
    }
}