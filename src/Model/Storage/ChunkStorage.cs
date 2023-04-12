using System.Text;
using MinecraftCloneSilk.Model.NChunk;

namespace MinecraftCloneSilk.Model.Storage;

public class ChunkStorage
{
    private readonly string pathToChunkFolder;

    public string PathToChunk(Chunk chunk) =>  pathToChunkFolder + "/" + chunk.position.X + "  " + chunk.position.Y  + "  " + chunk.position.Z;

    public ChunkStorage(string pathToChunkFolder) {
        this.pathToChunkFolder = pathToChunkFolder;
    }
    
    
    public void SaveChunk(Chunk chunk) {
        if(!chunk.blockModified) return;
        using Stream stream = File.Create(PathToChunk(chunk));
        using BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8, false);
        int version = 1;
        binaryWriter.Write(version);
        byte chunkState = (byte)chunk.chunkState;
        binaryWriter.Write(chunkState);
        int tick = 0; //Todo specify tick
        binaryWriter.Write(tick);
        Dictionary<int, BlockData> palette = new Dictionary<int, BlockData>();
        byte[] blockBytes = new byte[sizeof(Int32)  *  (Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE)];
        int index = 0;
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    BlockData blockData = chunk.blocks[x, y, z];
                    if(!palette.ContainsKey(blockData.id)) {
                        palette[blockData.id] = blockData;
                    }
                    blockBytes[index] = (byte) blockData.id;
                    blockBytes[index + 1] = (byte) (blockData.id >> 8);
                    blockBytes[index + 2] = (byte) (blockData.id >> 16);
                    blockBytes[index + 3] = (byte) (blockData.id >> 24);
                    index += 4;
                }
            }
        }
        byte[] bytesPallet = new byte[palette.Count * BlockData.sizeofSerializeData];
        BlockData[] blockDatasPalette = palette.Values.ToArray();
        for (int i = 0; i < blockDatasPalette.Length; i++) {
            bytesPallet[(i * 2)] = (byte) blockDatasPalette[i].id;
            bytesPallet[(i * 2) + 1] = (byte) (blockDatasPalette[i].id >> 8);
        }
        binaryWriter.Write(palette.Count);
        binaryWriter.Write(bytesPallet);
        if (palette.Count > 1) {
            binaryWriter.Write(blockBytes);
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