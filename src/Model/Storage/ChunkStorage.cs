using System.IO.Compression;
using System.Text;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Storage;

public class ChunkStorage
{
    private readonly string pathToChunkFolder;

    public string PathToChunk(Vector3D<int> position) =>
        $"{pathToChunkFolder}/{position.X}  {position.Y}  {position.Z}";

    public ChunkStorage(string pathToChunkFolder) {
        this.pathToChunkFolder = pathToChunkFolder;
        var directory = Directory.CreateDirectory(pathToChunkFolder);
        if(!directory.Exists) {
            throw new Exception("Can't create directory for chunk storage");
        }
    }
    
    
    public void SaveChunk(Chunk chunk) {
        if(!chunk.blockModified) return;
        using Stream stream = File.Create(PathToChunk(chunk.position));
        using ZLibStream zs = new ZLibStream(stream, CompressionLevel.Fastest, false);
        using BinaryWriter binaryWriter = new BinaryWriter(zs, Encoding.UTF8, false);
        int version = 1;
        binaryWriter.Write(version);
        byte chunkState = chunk.chunkState > ChunkState.BLOCKGENERATED ? (byte)ChunkState.BLOCKGENERATED : (byte)chunk.chunkState;
        binaryWriter.Write(chunkState);
        int tick = 0; //Todo specify tick
        binaryWriter.Write(tick);
        
        Dictionary<int, BlockData> palette = getPallette(chunk);
        
        byte[] bytesPallet = new byte[palette.Count * BlockData.sizeofSerializeData];
        BlockData[] blockDatasPalette = palette.Values.ToArray();
        for (int i = 0; i < blockDatasPalette.Length; i++) {
            for (int j = 0; j < BlockData.sizeofSerializeData; j++) {
                bytesPallet[(i * BlockData.sizeofSerializeData) + j] = (byte) (blockDatasPalette[i].id >> (8 * j));
            }
        }
        
        binaryWriter.Write(palette.Count);
        binaryWriter.Write(bytesPallet);

        int[] arrayOfKey = palette.Keys.ToArray();
        if (palette.Count > 1) {
            int maxIndex = palette.Count - 1;
            int bytesPerBlock = Log8Ceil(maxIndex);
            byte[] bytes = new byte[Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * bytesPerBlock];
            int index = 0;
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                        int indexPalette = Array.IndexOf(arrayOfKey, chunk.getBlockData(new Vector3D<int>(x, y, z)).id);
                        for (int i = 0; i < bytesPerBlock; i++) {
                            bytes[index] = (byte)(indexPalette >> (i * 8));
                            index++;
                        }
                    }
                }
            }
            binaryWriter.Write(bytes);
            
        }
        
        chunk.blockModified = false;
    }

    private Dictionary<int, BlockData> getPallette(Chunk chunk) {
        Dictionary<int, BlockData> palette = new Dictionary<int, BlockData>();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    BlockData blockData = chunk.blocks[x, y, z];
                    if(!palette.ContainsKey(blockData.id)) {
                        palette[blockData.id] = blockData;
                    }
                }
            }
        }

        return palette;
    }

    public bool isChunkExistInMemory(Vector3D<int> position) {
        return Directory.Exists(pathToChunkFolder) && File.Exists(PathToChunk(position));
    }


    public ChunkState getChunkStateInMemory(Vector3D<int> position) {
        if (!isChunkExistInMemory(position)) return ChunkState.EMPTY;
        using FileStream fs = File.Open(PathToChunk(position), FileMode.Open);
        using ZLibStream zs = new ZLibStream(fs, CompressionMode.Decompress, false);
        using BinaryReader br = new BinaryReader(zs);
        br.ReadInt32(); //version
        return (ChunkState) br.ReadByte();
    }
    
    public void LoadBlocks(Chunk chunk) {
        using FileStream fs = File.Open(PathToChunk(chunk.position), FileMode.Open);
        using ZLibStream zs = new ZLibStream(fs, CompressionMode.Decompress, false);
        using BinaryReader br = new BinaryReader(zs);
        br.ReadInt32(); //version
        br.ReadByte(); // chunkState
        br.ReadInt32(); // tick
        int nbBlockInPalette = br.ReadInt32();
        
        BlockData[] blocksData = new BlockData[nbBlockInPalette];
        for (int i = 0; i < nbBlockInPalette; i++) {
            blocksData[i] = new BlockData(br);
        }

        if (nbBlockInPalette > 1) {
            int nbBytePerBlock = Log8Ceil(nbBlockInPalette);
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                        int indexPalette = 0;
                        for (int j = 0; j < nbBytePerBlock; j++) {
                            indexPalette += br.ReadByte() << (j * 8);
                        }
                        chunk.blocks[x,y,z] = blocksData[indexPalette];
                    }
                }
            }   
        } else if(nbBlockInPalette == 1) {
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                        chunk.blocks[x,y,z] = blocksData[0];
                    }
                }
            }
        }
    }
    
    
    public static int Log8Ceil(int x)
    {
        int v = x ; // 32-bit word to find the log base 2 of
        int r = 0; // r will be lg(v)
        while (v > 0) // unroll for more speed...
        {
            v >>= 8;
            r++;
        }
        return r;
    }
}