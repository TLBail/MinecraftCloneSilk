using System.Text;
using MinecraftCloneSilk.Model.NChunk;

namespace MinecraftCloneSilk.Model.Storage;

public class ChunkStorage
{
    private readonly string pathToChunkFolder;

    protected string PathToChunk(Chunk chunk) =>  pathToChunkFolder + "/" + chunk.position.X + "  " + chunk.position.Y  + "  " + chunk.position.Z;

    public ChunkStorage(string pathToChunkFolder) {
        this.pathToChunkFolder = pathToChunkFolder;
    }
    
    
    public void SaveChunk(Chunk chunk) {
        if(!chunk.blockModified) return;
        using Stream stream = File.Create(PathToChunk(chunk));
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    BlockData blockData = chunk.blocks[x, y, z];
                    stream.Write(blockData.tobyte());
                }
            }
        }
        chunk.blockModified = false;
    }
    
    public bool isChunkExistInMemory(Chunk chunk) {
        return Directory.Exists(pathToChunkFolder) && File.Exists(PathToChunk(chunk));
    }

    
    public void LoadBlocks(Chunk chunk) {
        byte[] bytes = File.ReadAllBytes(PathToChunk(chunk));
        const int sizeofSerializeData = BlockData.sizeofSerializeData;
        const int expectedArrayLength = 16 * 16 * 16 * sizeofSerializeData;
        if (expectedArrayLength != bytes.Length) throw new GameException("Fail to load chunk from file " + chunk.position);
        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(bytes);
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    chunk.blocks[x, y, z] = new BlockData(span.Slice(0, sizeofSerializeData));
                    span = span.Slice(sizeofSerializeData);
                }
            }
        }
    }
}