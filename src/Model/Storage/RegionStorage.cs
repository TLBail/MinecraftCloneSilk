using System.Text;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Storage;



public class RegionStorage : IChunkStorage
{

    public struct RegionFile
    {
        public RegionHeaderEntry[,,] headerEntries = new RegionHeaderEntry[REGION_SIZE,REGION_SIZE,REGION_SIZE];
        public int startingFreeSpaceOffset = REGION_SIZE * REGION_SIZE * REGION_SIZE * RegionHeaderEntry.SIZE;
        
        public Chunk?[,,] chunks = new Chunk?[REGION_SIZE , REGION_SIZE , REGION_SIZE];
        public RegionFile() { }

    }
    
    public struct RegionHeaderEntry
    {
        public const int SIZE = 8;
        
        private byte position1;
        private byte position2;
        private byte position3;
        
        public byte nbBlock; // one block = 4096 bytes

        public int timestamp;
        
        public int getPosition() {
            return position1 << 16 | position2 << 8 | position3;
        }
        
        public void setPosition(int position) {
            position1 = (byte) (position >> 16);
            position2 = (byte) (position >> 8);
            position3 = (byte) position;
        }
        
        public void writeToStream(BinaryWriter stream) {
            stream.Write(position1);
            stream.Write(position2);
            stream.Write(position3);
            stream.Write(nbBlock);
            stream.Write(timestamp);
        }
        
        
        public static RegionHeaderEntry readFromStream(BinaryReader stream) {
            RegionHeaderEntry entry = new RegionHeaderEntry();
            entry.position1 = stream.ReadByte();
            entry.position2 = stream.ReadByte();
            entry.position3 = stream.ReadByte();
            entry.nbBlock = stream.ReadByte();
            entry.timestamp = stream.ReadInt32();
            return entry;
        }

        public override bool Equals(object? obj) {
            return obj is RegionHeaderEntry entry &&
                   position1 == entry.position1 &&
                   position2 == entry.position2 &&
                   position3 == entry.position3 &&
                   nbBlock == entry.nbBlock &&
                   timestamp == entry.timestamp;
        }
    }
    
    private readonly string pathToChunkFolder;
    public const int REGION_SIZE = 16;
    public const int NBHEADERENTRY = 16 * 16 * 16;
    public string PathToRegion(Vector3D<int> position) =>
        $"{pathToChunkFolder}/{position.X}.{position.Y}.{position.Z}.mcr";

    public RegionStorage(string pathToChunkFolder) {
        this.pathToChunkFolder = pathToChunkFolder;
        var directory = Directory.CreateDirectory(pathToChunkFolder);
        if(!directory.Exists) {
            throw new Exception("Can't create directory for chunk storage");
        }

    }
    
    public void saveRegion(Vector3D<int> position, RegionFile regionFile) {
        using Stream stream = File.Open(PathToRegion(position), FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8, false);
        binaryWriter.Seek(regionFile.startingFreeSpaceOffset, SeekOrigin.Begin);
        
        MemoryStream memoryStreamOverflow = new MemoryStream();
        bool overflow = false;

        for (int i = 0; i < REGION_SIZE; i++) {
            for (int j = 0; j < REGION_SIZE; j++) {
                for (int k = 0; k < REGION_SIZE; k++) {
                    if (regionFile.chunks[i, j, k] is not null) {
                        if(regionFile.headerEntries[i,j,k].nbBlock == 0) {
                            regionFile.headerEntries[i,j,k].setPosition((int)stream.Position);
                            ChunkStorage.SaveChunk(stream, regionFile.chunks[i, j, k]!);
                            regionFile.headerEntries[i,j,k].nbBlock = (byte)(Math.Ceiling((stream.Position - regionFile.headerEntries[i,j,k].getPosition()) / 4096d));
                            regionFile.headerEntries[i,j,k].timestamp = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
                        
                            // seek to a multiple of 4096
                            stream.Seek(4096 - (stream.Position % 4096), SeekOrigin.Current);
                            regionFile.startingFreeSpaceOffset = (int)stream.Position;
                        } else {
                            int previousSize = regionFile.headerEntries[i,j,k].nbBlock * 4096;
                            MemoryStream memoryStream = new MemoryStream(previousSize);
                            ChunkStorage.SaveChunk(memoryStream, regionFile.chunks[i, j, k]!);
                            int startPosition = regionFile.headerEntries[i,j,k].getPosition();
                            stream.Seek(startPosition, SeekOrigin.Begin);
                            if (memoryStream.Length > previousSize) {
                                // si il n'y a pas de chunk après
                                if(stream.Position + memoryStream.Length > stream.Length) {
                                    stream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                                    stream.Seek(4096 - (stream.Position % 4096), SeekOrigin.Current);
                                    regionFile.startingFreeSpaceOffset = (int)stream.Position;
                                } else {
                                    // Sauvegarde des données après le chunk actuel
                                    long nextChunkPos = stream.Position + previousSize;
                                    stream.Seek(nextChunkPos, SeekOrigin.Begin);
                                    byte[] dataAfter = new byte[stream.Length - stream.Position];
                                    stream.Read(dataAfter, 0, dataAfter.Length);

                                    // Réécriture du chunk agrandi
                                    stream.Seek(startPosition, SeekOrigin.Begin);
                                
                                    stream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                                
                                    stream.Seek(4096 - (stream.Position % 4096), SeekOrigin.Current);
                                    stream.Write(dataAfter, 0, dataAfter.Length);
                                    stream.Seek(4096 - (stream.Position % 4096), SeekOrigin.Current);
                                    regionFile.startingFreeSpaceOffset = (int)stream.Position;
                                    //Todo update header entry of all moved chunk
                                }
                                
                            } else {
                                stream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                            }
                            stream.Seek(regionFile.startingFreeSpaceOffset, SeekOrigin.Begin);
                        }
                    }
                }
            }    
        }
        binaryWriter.Seek(0, SeekOrigin.Begin);
        
        for (int i = 0; i < REGION_SIZE; i++) {
            for (int j = 0; j < REGION_SIZE; j++) {
                for (int k = 0; k < REGION_SIZE; k++) {
                    regionFile.headerEntries[i,j,k].writeToStream(binaryWriter);                    
                }
            }    
        }
        
      
    }

    public RegionFile getRegionFromPosition(Vector3D<int> position) {
        string pathToRegion = PathToRegion(position);
        if (!File.Exists(pathToRegion)) return new RegionFile();
        using FileStream fs = File.Open(pathToRegion, FileMode.Open, FileAccess.Read);
        using BinaryReader binaryReader = new BinaryReader(fs, Encoding.UTF8, false);
        RegionFile regionFile = new RegionFile();
        for (int i = 0; i < REGION_SIZE; i++) {
            for (int j = 0; j < REGION_SIZE; j++) {
                for (int k = 0; k < REGION_SIZE; k++) {
                    RegionHeaderEntry entry = RegionHeaderEntry.readFromStream(binaryReader);
                    int freeSpaceOffset = entry.getPosition() + (entry.nbBlock * 4096);
                    if (freeSpaceOffset > regionFile.startingFreeSpaceOffset) {
                        regionFile.startingFreeSpaceOffset = freeSpaceOffset;
                    }
                    regionFile.headerEntries[i, j, k] = entry;
                }
            }    
        }
        return regionFile;
    }


    public string getRegionFileFromPosition(Vector3D<int> chunkPosition) {
        return PathToRegion(getRegionPosition(chunkPosition));
    }

    public Vector3D<int> getRegionPosition(Vector3D<int> chunkPosition) {
        return new Vector3D<int>((int)Math.Floor((double)chunkPosition.X / (REGION_SIZE* Chunk.CHUNK_SIZE)),
            (int)Math.Floor((double)chunkPosition.Y / (REGION_SIZE* Chunk.CHUNK_SIZE)),
            (int)Math.Floor((double)chunkPosition.Z / (REGION_SIZE* Chunk.CHUNK_SIZE))
        );
    }

    public void SaveChunk(Chunk chunk) {
        RegionFile regionFile = getRegionFromPosition(getRegionPosition(chunk.position));
        Vector3D<int> localPosition = getLocalChunkPosition(chunk.position);
        regionFile.chunks[localPosition.X,localPosition.Y,localPosition.Z] = chunk;
        saveRegion(getRegionPosition(chunk.position), regionFile);
    }
    
    public void SaveChunks(List<Chunk> chunks) {
        if(chunks.Count == 0) return;
        Dictionary<Vector3D<int>, RegionFile> regionfiles = new Dictionary<Vector3D<int>, RegionFile>();
        foreach (Chunk chunk in chunks) {
            if(GetChunkStateInStorage(chunk.position) > chunk.chunkState) continue;
            
            Vector3D<int> regionPosition = getRegionPosition(chunk.position);
            if (!regionfiles.ContainsKey(regionPosition)) {
                regionfiles.Add(regionPosition, getRegionFromPosition(regionPosition));
            }
            Vector3D<int> localPosition = getLocalChunkPosition(chunk.position);
            regionfiles[regionPosition].chunks[localPosition.X,localPosition.Y,localPosition.Z] = chunk;
        }
        foreach (Vector3D<int> regionFilePosition in regionfiles.Keys) {
            saveRegion(getRegionPosition(regionFilePosition), regionfiles[regionFilePosition]);
        }
    }
    
    
    

    public void LoadChunk(Chunk chunk) {
        RegionFile regionFile = getRegionFromPosition(getRegionPosition(chunk.position));
        Vector3D<int> localPosition = getLocalChunkPosition(chunk.position);
        if (regionFile.headerEntries[localPosition.X, localPosition.Y,
                localPosition.Z].nbBlock == 0) {
            throw new Exception("Chunk not found");   
        } 
        using FileStream fs = File.Open(PathToRegion(getRegionPosition(chunk.position)), FileMode.Open, FileAccess.Read);
        fs.Seek(regionFile.headerEntries[localPosition.X, localPosition.Y,localPosition.Z].getPosition(), SeekOrigin.Begin);
        ChunkStorage.LoadBlocks(fs, chunk);
    }


    public void LoadChunks(List<Chunk> chunks) {
        if(chunks.Count == 0) return;
        foreach (Chunk chunk in chunks) {
            LoadChunk(chunk);
        }
    }

    public ChunkState GetChunkStateInStorage(Vector3D<int> position) {
        RegionFile regionFile = getRegionFromPosition(getRegionPosition(position));
        Vector3D<int> localPosition = getLocalChunkPosition(position);
        if (regionFile.headerEntries[localPosition.X, localPosition.Y,localPosition.Z].nbBlock == 0) {
            return ChunkState.EMPTY;
        } 
        using FileStream fs = File.Open(PathToRegion(getRegionPosition(position)), FileMode.Open, FileAccess.Read);
        fs.Seek(regionFile.headerEntries[localPosition.X, localPosition.Y,localPosition.Z].getPosition(), SeekOrigin.Begin);
        return ChunkStorage.GetChunkStateInStorage(fs);
    }

    public bool isChunkExistInMemory(Vector3D<int> position) {
        RegionFile regionFile = getRegionFromPosition(getRegionPosition(position));
        Vector3D<int> localPosition = getLocalChunkPosition(position);
        return regionFile.headerEntries[ localPosition.X, localPosition.Y, localPosition.Z].nbBlock != 0;
    }

    
    public static Vector3D<int> getLocalChunkPosition(Vector3D<int> globalPosition) {
        var localPosition = new Vector3D<int>((int)(globalPosition.X % (REGION_SIZE* Chunk.CHUNK_SIZE)),
            (int)(globalPosition.Y % (REGION_SIZE * Chunk.CHUNK_SIZE)), (int)(globalPosition.Z % (REGION_SIZE * Chunk.CHUNK_SIZE)));
        localPosition /= REGION_SIZE ;
        if (localPosition.X < 0) {
            localPosition.X = (int)(REGION_SIZE + localPosition.X);
        }

        if (localPosition.Y < 0) {
            localPosition.Y = (int)(REGION_SIZE + localPosition.Y);
        }

        if (localPosition.Z < 0) {
            localPosition.Z = (int)(REGION_SIZE + localPosition.Z);
        }

        return localPosition;
    }
    
}