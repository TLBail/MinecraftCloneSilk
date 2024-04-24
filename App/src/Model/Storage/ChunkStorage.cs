using System.Diagnostics;
using System.IO.Compression;
using System.Numerics;
using System.Text;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Storage;

public class ChunkStorage : IChunkStorage
{
    private readonly string pathToChunkFolder;
    
    public const int CURRENT_VERSION = 2;


    public string PathToChunk(Vector3D<int> position) =>
        $"{pathToChunkFolder}/{position.X}  {position.Y}  {position.Z}";

    public ChunkStorage(string pathToChunkFolder) {
        this.pathToChunkFolder = pathToChunkFolder;
        var directory = Directory.CreateDirectory(pathToChunkFolder);
        if(!directory.Exists) {
            throw new Exception("Can't create directory for chunk storage");
        }
    }


    public void SaveChunkAsync(Chunk chunk) {
        chunk.AddRequiredByChunkSaver();
        Task task = new Task(() =>
        {
            SaveChunk(chunk);
            chunk.RemoveRequiredByChunkSaver();
        });
        task.Start();
    }

    public void SaveChunk(Chunk chunk) {
        using Stream stream = File.Create(PathToChunk(chunk.position));
        using ZLibStream zs = new ZLibStream(stream, CompressionLevel.Fastest, false);
        SaveChunk(zs, chunk);
    }

    public void SaveChunks(List<Chunk> chunks) {
        foreach (Chunk chunk in chunks) {
            SaveChunk(chunk);
        }
    }

    public static void SaveChunk(Stream stream, Chunk chunk) {
        using BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8, true);
        int version = CURRENT_VERSION;
        binaryWriter.Write(version);
        Debug.Assert(!ChunkStateTools.IsChunkIsLoading(chunk.chunkState), "try to save a chunk that is loading");
        Debug.Assert(chunk.chunkState != ChunkState.EMPTY || chunk.chunkState != ChunkState.UNKNOW, "try to save a chunk that have a strange state");
        byte chunkState = (byte)BitOperations.Log2((uint)(chunk.chunkState > ChunkState.BLOCKGENERATED ? ChunkState.BLOCKGENERATED : chunk.chunkState));
        binaryWriter.Write(chunkState);
        int tick = 0; //Todo specify tick
        binaryWriter.Write(tick);
        
        Dictionary<int, BlockData> palette = GetPallette(chunk);
        
        binaryWriter.Write((int)palette.Count);
        foreach (BlockData blockData in palette.Values) {
            blockData.WriteToStream(binaryWriter);
        }

        int[] arrayOfKey = palette.Keys.ToArray();
        if (palette.Count > 1) {
            int maxIndex = palette.Count - 1;
            int bytesPerBlock = Log8Ceil(maxIndex);
            Span<byte> bytes = stackalloc byte[Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * bytesPerBlock];
            int index = 0;
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                        int indexPalette = Array.IndexOf(arrayOfKey, chunk.GetBlockData(new Vector3D<int>(x, y, z)).id);
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
    
    

    private static Dictionary<int, BlockData> GetPallette(Chunk chunk) {
        Dictionary<int, BlockData> palette = new Dictionary<int, BlockData>();
        
        BlockData[,,] blocks = chunk.chunkData.GetBlocks();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    BlockData blockData = blocks[x, y, z];
                    if (!palette.ContainsKey(blockData.id)) {
                        blockData.SetLightLevel(Chunk.blockFactory.blocks[blockData.id].lightEmitting);
                        palette.TryAdd(blockData.id, blockData);
                    }
                }
            }
        }

        return palette;
    }

    public bool IsChunkExistInMemory(Vector3D<int> position) {
        return Directory.Exists(pathToChunkFolder) && File.Exists(PathToChunk(position));
    }


    public ChunkState GetChunkStateInStorage(Vector3D<int> position) {
        if (!IsChunkExistInMemory(position)) return ChunkState.EMPTY;
        using FileStream fs = File.Open(PathToChunk(position), FileMode.Open);
        using ZLibStream zs = new ZLibStream(fs, CompressionMode.Decompress, false);
        return GetChunkStateInStorage(zs);
    }
    
    
    public static ChunkState GetChunkStateInStorage(Stream stream) {
        using BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true);
        if(br.ReadInt32() != CURRENT_VERSION) throw new Exception("bad version of chunk");
        return (ChunkState) (1 << br.ReadByte());
    }
    
    
    public void LoadChunk(Chunk chunk) {
        using FileStream fs = File.Open(PathToChunk(chunk.position), FileMode.Open);
        using ZLibStream zs = new ZLibStream(fs, CompressionMode.Decompress, false);
        LoadBlocks(zs, chunk);
    }

    public void LoadChunks(List<Chunk> chunks) {
        foreach (Chunk chunk in chunks) {
            LoadChunk(chunk);
        }
    }

    public static void LoadBlocks(Stream stream, Chunk chunk) {
        using BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true);
        if(br.ReadInt32() != CURRENT_VERSION) throw new Exception("bad version of chunk");
        br.ReadByte(); // chunkState
        br.ReadInt32(); // tick
        

        int nbBlockInPalette = br.ReadInt32();
        
        BlockData[] blocksData = new BlockData[nbBlockInPalette];
        for (int i = 0; i < nbBlockInPalette; i++) {
            blocksData[i] = new BlockData(br);
        }

        if (nbBlockInPalette > 1) {
            BlockData[,,] blocks = chunk.chunkData.GetBlocks();
            int nbBytePerBlock = Log8Ceil(nbBlockInPalette);
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                        int indexPalette = 0;
                        for (int j = 0; j < nbBytePerBlock; j++) {
                            indexPalette += br.ReadByte() << (j * 8);
                        }
                        blocks[x,y,z] = blocksData[indexPalette];
                    }
                }
            }   
        } else if(nbBlockInPalette == 1) {
            chunk.chunkData.SetBlocks(blocksData[0]);
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
    
    
    public void Dispose() { }
}