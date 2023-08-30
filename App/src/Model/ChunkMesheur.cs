using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.NChunk;

public static class ChunkMesheur
{
    
    public static int GetVerticesCount(Chunk chunk) {
        Vector3D<float> positionFloat =
            new Vector3D<float>(chunk.position.X, chunk.position.Y, chunk.position.Z);
        List<CubeVertex> vertices = new List<CubeVertex>();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    BlockData block = chunk.blocks[x, y, z];
                    if (block.id == 0) continue;
                    FaceFlag faces = GetFaces(chunk,x, y, z);
                    if (faces > 0) {
                        Chunk.blockFactory.blocksReadOnly[block.id].textureBlock!.AddCubeVerticesToList(vertices, faces, new Vector3D<float>(x, y, z), positionFloat);
                    }
                }
            }
        }
        return vertices.Count;
    }

    private static FaceFlag GetFaces(Chunk chunk,  int x, int y, int z) {
        FaceFlag faceFlag = FaceFlag.EMPTY;
        if (IsBlockTransparent(chunk,x - 1, y, z)) {
            //X
            faceFlag |= FaceFlag.RIGHT;
        }

        if (IsBlockTransparent(chunk,x + 1, y, z)) {
            faceFlag |= FaceFlag.LEFT;
        }

        if (IsBlockTransparent(chunk,x, y - 1, z)) {
            // Y
            faceFlag |= FaceFlag.BOTTOM;
        }

        if (IsBlockTransparent(chunk,x, y + 1, z)) {
            faceFlag |= FaceFlag.TOP;
        }

        if (IsBlockTransparent(chunk,x, y, z - 1)) {
            // Z
            faceFlag |= FaceFlag.BACK;
        }

        if (IsBlockTransparent(chunk,x, y, z + 1)) {
            faceFlag |= FaceFlag.FRONT;
        }

        return faceFlag;
    }

    private static bool IsBlockTransparent(Chunk chunk, int x, int y, int z) {
        BlockData blockData;
        if (y < 0) {
            blockData = chunk.chunksNeighbors![(int)Face.BOTTOM]
                .GetBlockData(new Vector3D<int>(x, y + (int)Chunk.CHUNK_SIZE, z));
        } else if (y >= Chunk.CHUNK_SIZE) {
            blockData = chunk.chunksNeighbors![(int)Face.TOP]
                .GetBlockData(new Vector3D<int>(x, y - (int)Chunk.CHUNK_SIZE, z));
        } else if (x < 0) {
            blockData = chunk.chunksNeighbors![(int)Face.LEFT]
                .GetBlockData(new Vector3D<int>(x + (int)Chunk.CHUNK_SIZE, y, z));
        } else if (x >= Chunk.CHUNK_SIZE) {
            blockData = chunk.chunksNeighbors![(int)Face.RIGHT]
                .GetBlockData(new Vector3D<int>(x - (int)Chunk.CHUNK_SIZE, y, z));
        } else if (z < 0) {
            blockData = chunk.chunksNeighbors![(int)Face.BACK]
                .GetBlockData(new Vector3D<int>(x, y, z + (int)Chunk.CHUNK_SIZE));
        } else if (z >= Chunk.CHUNK_SIZE) {
            blockData = chunk.chunksNeighbors![(int)Face.FRONT]
                .GetBlockData(new Vector3D<int>(x, y, z - (int)Chunk.CHUNK_SIZE));
        } else {
            blockData = chunk.blocks[x, y, z];
        }

        return blockData.id == 0 || Chunk.blockFactory!.IsBlockTransparent(blockData);
    }
}