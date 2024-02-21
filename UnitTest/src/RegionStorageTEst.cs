using System.Reflection;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;
using UnitTest.fakeClass;

namespace UnitTest;

public class RegionStorageTEst
{
    
    private ChunkManagerEmpty chunkManagerEmpty = null!;
    private ChunkLoader chunkLoader = null!;
    private RegionStorage regionStorage = null!;
    
    [OneTimeSetUp]
    public void SetUp() {
        Chunk.InitStaticMembers(null, BlockFactory.GetInstance());
        DirectoryInfo directory = Directory.CreateDirectory("./Worlds/newWorld");
        foreach (var file in Directory.GetFiles("./Worlds/newWorld")) {
            File.Delete(file);
        }   
        regionStorage = new RegionStorage("./Worlds/newWorld");
        chunkManagerEmpty = new ChunkManagerEmpty(new WorldFlatGeneration(), regionStorage);
        chunkLoader = new ChunkLoader(ChunkLoader.ChunkLoaderMode.SYNC);
    }
    
    [OneTimeTearDown]
    public void TearDown() {
        regionStorage.Dispose();
    }
    
    
    [Test]
    public void TestCreatingRegionStorage() {
        RegionStorage regionStorage = new RegionStorage("./Worlds/newWorld");
        Assert.NotNull(regionStorage);
    }

    [Test]
    public void AbleToGetBlockGeneratedChunk() {
        Chunk chunk = getBlockGeneratedChunk();
        Assert.That(chunk.chunkState, Is.EqualTo(ChunkState.BLOCKGENERATED));
    }

    [Test]
    public void SaveChunk() {
        RegionStorage regionStorage = new RegionStorage("./Worlds/newWorld");
        Chunk chunk = getBlockGeneratedChunk();
        regionStorage.SaveChunk(chunk);
        Assert.True(true);
    }


    [Test]
    public void TestSaveAndLoadChunk() {
        RegionStorage regionStorage = new RegionStorage("./Worlds/newWorld");
        Chunk chunk = getBlockGeneratedChunk();
        chunk.SetBlock(0,0,0, "metal");
        regionStorage.SaveChunk(chunk);
        chunkManagerEmpty.RemoveChunk(Vector3D<int>.Zero);

        Chunk chunkEmpty = chunkManagerEmpty.GetChunk(Vector3D<int>.Zero);
        Assert.That(chunkEmpty.chunkState, Is.EqualTo(ChunkState.EMPTY));
        regionStorage.LoadChunk(chunkEmpty);
        
        FieldInfo fi = typeof(Chunk).GetField("chunkData",    BindingFlags.NonPublic | BindingFlags.Instance)!;
        IChunkData chunkData = (IChunkData)fi.GetValue(chunkEmpty)!;
        BlockData[,,] blocks = chunkData.GetBlocks();
        Block block =BlockFactory.GetInstance().BuildFromBlockData(new Vector3D<int>(0, 0, 0), blocks[0, 0, 0]);
        Assert.That(block.name, Is.EqualTo("metal"));
    }
    
    
    [Test]
    public void TestSaveAndLoadChunkNotZeroZero() {
        Vector3D<int> position = new Vector3D<int>(321432 * Chunk.CHUNK_SIZE, 445 * Chunk.CHUNK_SIZE, -123 * Chunk.CHUNK_SIZE);

        RegionStorage regionStorage = new RegionStorage("./Worlds/newWorld");
        Chunk chunk = getBlockGeneratedChunk(position);
        chunk.SetBlock(0,0,0, "metal");
        regionStorage.SaveChunk(chunk);
        chunkManagerEmpty.RemoveChunk(position);

        Chunk chunkEmpty = chunkManagerEmpty.GetChunk(position);
        Assert.That(chunkEmpty.chunkState, Is.EqualTo(ChunkState.EMPTY));
        regionStorage.LoadChunk(chunkEmpty);
        
        FieldInfo fi = typeof(Chunk).GetField("chunkData",    BindingFlags.NonPublic | BindingFlags.Instance)!;
        IChunkData chunkData = (IChunkData)fi.GetValue(chunkEmpty)!;
        BlockData[,,] blocks = chunkData.GetBlocks();
        Block block =BlockFactory.GetInstance().BuildFromBlockData(new Vector3D<int>(0, 0, 0), blocks[0, 0, 0]);
        Assert.That(block.name, Is.EqualTo("metal"));
    }
    
    [Test]
    public void TestCreatedChunkIsAbleToSaveAndRecoverData() {
        Vector3D<int> position = new Vector3D<int>(1 * Chunk.CHUNK_SIZE, 1 * Chunk.CHUNK_SIZE, 0);
        Chunk chunk = getBlockGeneratedChunk(position);
        chunk.SetBlock(0,0,0, "metal");
    
        regionStorage.SaveChunk(chunk);
            
        chunkManagerEmpty.RemoveChunk(position);
            
        Chunk chunk2 = getBlockGeneratedChunk(position);
    
            
        Assert.True(chunk2.GetBlock(Vector3D<int>.Zero).name.Equals("metal"));
    }
        


    [Test]
    public void testSaveMultipleChunk() {

        RegionStorage regionStorage = new RegionStorage("./Worlds/newWorld");
        
        
        
        Span<Vector3D<int>> positions = stackalloc Vector3D<int>[]{
            new(0,0,0),
            new( Chunk.CHUNK_SIZE,0,0),
            new( 2 * Chunk.CHUNK_SIZE, 0, 0)
        };
        String[] blocksToSet = new[]
        {
            "metal",
            "dirt",
            "oak"
        };
        
        List<Chunk> chunks = new();
        int index = 0;
        foreach (Vector3D<int> position in positions) {
            Chunk chunk = getBlockGeneratedChunk(position);
            chunk.SetBlock(0,0,0, blocksToSet[index++]);
            chunks.Add(chunk);
            chunkManagerEmpty.RemoveChunk(position);   
            regionStorage.SaveChunks(new (){chunk});
        }

        chunkManagerEmpty.chunks.Clear();

        List<Chunk> newChunks = new();
        foreach (Vector3D<int> position in positions) {
            Chunk chunkEmpty = chunkManagerEmpty.GetChunk(position);
            Assert.That(chunkEmpty.chunkState, Is.EqualTo(ChunkState.EMPTY));
            newChunks.Add(chunkManagerEmpty.GetChunk(position));
        }    
        
        regionStorage.LoadChunks(newChunks);
        FieldInfo fi = typeof(Chunk).GetField("chunkData",    BindingFlags.NonPublic | BindingFlags.Instance)!;
        index = 0;
        foreach (Chunk chunk in newChunks) {
            IChunkData chunkData = (IChunkData)fi.GetValue(chunk)!;
            BlockData[,,] blocks = chunkData.GetBlocks();
            Block block =BlockFactory.GetInstance().BuildFromBlockData(new Vector3D<int>(0, 0, 0), blocks[0, 0, 0]);
            Assert.That(block.name, Is.EqualTo(blocksToSet[index++]));  
        }
        
    }

    [Test]
    public void testSaveMultipleChunkInOneRegion() {
        RegionStorage regionStorage = new RegionStorage("./Worlds/newWorld");
        Span<Vector3D<int>> positions = stackalloc Vector3D<int>[]
        {
            new(0, 0, 0),
            new(Chunk.CHUNK_SIZE, 0, 0),
            new(2 * Chunk.CHUNK_SIZE, 0, 0)
        };
        Chunk[] chunks = new[]
        {
            getBlockGeneratedChunk(positions[0]),
            getBlockGeneratedChunk(positions[1])
        };
        chunks[0].SetBlock(0,0,0,"metal");
        chunks[1].SetBlock(14, 0, 3, "oak");
        this.regionStorage.SaveChunk(chunks[0]);
        this.regionStorage.SaveChunk(chunks[1]);
        Chunk chunk = new Chunk(positions[0], chunkManagerEmpty, new WorldFlatGeneration(), this.regionStorage);
        regionStorage.LoadChunk(chunk);
        
        FieldInfo fi = typeof(Chunk).GetField("chunkData",    BindingFlags.NonPublic | BindingFlags.Instance)!;
        IChunkData chunkData = (IChunkData)fi.GetValue(chunk)!;
        BlockData[,,] blocks = chunkData.GetBlocks();
        Block block =BlockFactory.GetInstance().BuildFromBlockData(new Vector3D<int>(0, 0, 0), blocks[0, 0, 0]);
        Assert.That(block.name, Is.EqualTo("metal"));  
        
        
        
        chunk = new Chunk(positions[1], chunkManagerEmpty, new WorldFlatGeneration(), this.regionStorage);
        regionStorage.LoadChunk(chunk);
        

        chunkData = (IChunkData)fi.GetValue(chunk)!;
        blocks = chunkData.GetBlocks();
        block =BlockFactory.GetInstance().BuildFromBlockData(new Vector3D<int>(14, 0, 3), blocks[14, 0, 3]);
        Assert.That(block.name, Is.EqualTo("oak"));  
    }
    
 
    
    [Test]
    public void TestCreatedChunksIsAbleToSaveAndRecoverData() {
        List<Vector3D<int>> positions = new()
        {
            new(-1 * Chunk.CHUNK_SIZE,0,0),
            new(-1 * Chunk.CHUNK_SIZE,-1 * Chunk.CHUNK_SIZE,0),
            new(-1 * Chunk.CHUNK_SIZE,-1 * Chunk.CHUNK_SIZE,-1 * Chunk.CHUNK_SIZE),
            new(1 * Chunk.CHUNK_SIZE,0,0),
            new(1 * Chunk.CHUNK_SIZE,-1 * Chunk.CHUNK_SIZE,0),
            new(-1 * Chunk.CHUNK_SIZE,-1 * Chunk.CHUNK_SIZE,1 * Chunk.CHUNK_SIZE),
        };
        List<Chunk> chunksToSave = new();
        foreach (Vector3D<int> position in positions) {
            Chunk chunk = getBlockGeneratedChunk(position);
            chunk.SetBlock(0,0,0, "metal");
            chunksToSave.Add(chunk);
        }

        foreach (Chunk chunk in chunksToSave) {
            Assert.True(chunk.GetBlock(Vector3D<int>.Zero).name.Equals("metal"));
        }
        
        
        regionStorage.SaveChunks(chunksToSave);
        foreach (Chunk chunk in chunksToSave) {
            chunkManagerEmpty.RemoveChunk(chunk.position);
        }
        

        foreach (Vector3D<int> position in positions) {
            Chunk chunk2 = getBlockGeneratedChunk(position);
            Assert.True(chunk2.GetBlock(Vector3D<int>.Zero).name.Equals("metal"));   
        }
    }

    
    
    
    [Test]
    public void TestCreatedChunkIsAbleToSaveAndRecoverDataWithNeigborsChunk() {
        Chunk chunk = getBlockGeneratedChunk();
        chunk.SetBlock(0,0,0, "metal");
        
        
        regionStorage.SaveChunks(chunkManagerEmpty.chunks.Values.ToList());
        
        chunkManagerEmpty.chunks.Clear();
        
        Chunk chunk2 = getBlockGeneratedChunk();
        Assert.True(chunk2.GetBlock(Vector3D<int>.Zero).name.Equals("metal"));   
    }
    
    [Test]
    public void TestCreatedChunksIsAbleToSaveAndRecoverDataWithNeigborsChunk() {
      
        Dictionary<Vector3D<int>, string> positionAnbBlockName = new()
        {
            {new(-1 * Chunk.CHUNK_SIZE,0,0), "metal"},
            {new(-1 * Chunk.CHUNK_SIZE,-1 * Chunk.CHUNK_SIZE,0), "dirt"},
            {new(-1 * Chunk.CHUNK_SIZE,-1 * Chunk.CHUNK_SIZE,-1 * Chunk.CHUNK_SIZE), "oak"},
            {new(1 * Chunk.CHUNK_SIZE,-1 * Chunk.CHUNK_SIZE,0), "metal"},
            {new(-1 * Chunk.CHUNK_SIZE,-1 * Chunk.CHUNK_SIZE,1 * Chunk.CHUNK_SIZE), "metal"},

            {new(1 * Chunk.CHUNK_SIZE,0,0), "metal"},
            {new(2 * Chunk.CHUNK_SIZE,0,0), "foliage"},
            {new(3 * Chunk.CHUNK_SIZE,0,0), "dirt"},
            {new(4 * Chunk.CHUNK_SIZE,0,0), "metal"},
            {new(5 * Chunk.CHUNK_SIZE,0,0), "dirt"},
            {new(6 * Chunk.CHUNK_SIZE,0,0), "foliage"},
            
        };

        List<Chunk> chunksToSave = new();
        foreach (Vector3D<int> position in positionAnbBlockName.Keys) {
            Chunk chunk = getBlockGeneratedChunk(position);
            chunk.SetBlock(0,0,0, positionAnbBlockName[position]);
            chunksToSave.Add(chunk);
        }

        foreach (Chunk chunk in chunksToSave) {
            Assert.True(chunk.GetBlock(Vector3D<int>.Zero).name.Equals(positionAnbBlockName[chunk.position]));
        }
        
        
        regionStorage.SaveChunks(chunkManagerEmpty.chunks.Values.ToList());
        chunkManagerEmpty.chunks.Clear();

        
        foreach (Vector3D<int> position in positionAnbBlockName.Keys) {
            Chunk chunk2 = getBlockGeneratedChunk(position);
            Assert.True(chunk2.GetBlock(Vector3D<int>.Zero).name.Equals(positionAnbBlockName[chunk2.position]));   
        }
    }
    
    [Test]
    public void testUpdateExistingChunk() {
        RegionStorage regionStorage = new RegionStorage("./Worlds/newWorld");
        Span<Vector3D<int>> positions = stackalloc Vector3D<int>[]
        {
            new(0, 0, 0),
            new(Chunk.CHUNK_SIZE, 0, 0)
        };
        Chunk[] chunks = new[]
        {
            getBlockGeneratedChunk(positions[0]),
            getBlockGeneratedChunk(positions[1])
        };
        chunks[1].SetBlock(14, 0, 3, "oak");
        this.regionStorage.SaveChunk(chunks[0]);
        this.regionStorage.SaveChunk(chunks[1]);
        
        Assert.That(chunks[0].GetBlock(0,0,0).name, Is.Not.EqualTo("oak"));
        chunks[0].SetBlock(0, 0, 3, "metal");
        
        this.regionStorage.SaveChunk(chunks[0]);
        
        Chunk chunk = new Chunk(positions[0], chunkManagerEmpty, new WorldFlatGeneration(), this.regionStorage);
        regionStorage.LoadChunk(chunk);
        FieldInfo fi = typeof(Chunk).GetField("chunkData",    BindingFlags.NonPublic | BindingFlags.Instance)!;
        IChunkData chunkData = (IChunkData)fi.GetValue(chunk)!;
        BlockData[,,] blocks = chunkData.GetBlocks();
        Block block =BlockFactory.GetInstance().BuildFromBlockData(new Vector3D<int>(0,0,3), blocks[0,0,3]);
        Assert.That(block.name, Is.EqualTo("metal"));  
        
        
        Chunk chunk2 = new Chunk(positions[1], chunkManagerEmpty, new WorldFlatGeneration(), this.regionStorage);
        regionStorage.LoadChunk(chunk2);
        chunkData = (IChunkData)fi.GetValue(chunk2)!;
        blocks = chunkData.GetBlocks();
        Block block2 =BlockFactory.GetInstance().BuildFromBlockData(new Vector3D<int>(14,0,3), blocks[14,0,3]); 
        Assert.That(block2.name, Is.EqualTo("oak"));
        
    } 
    
    [Test]
    public void testUpdateExistingChunks() {
        RegionStorage regionStorage = new RegionStorage("./Worlds/newWorld");
        Span<Vector3D<int>> positions = stackalloc Vector3D<int>[]
        {
            new(0, 0, 0),
            new(Chunk.CHUNK_SIZE, 0, 0),
            new(2 * Chunk.CHUNK_SIZE, 0, 0),
            new(3 * Chunk.CHUNK_SIZE, 0, 0),
        };
        Chunk[] chunks = new[]
        {
            getBlockGeneratedChunk(positions[0]),
            getBlockGeneratedChunk(positions[1]),
            getBlockGeneratedChunk(positions[2]),
            getBlockGeneratedChunk(positions[3])
        };
        chunks[1].SetBlock(14, 0, 3, "oak");
        this.regionStorage.SaveChunks(new (){chunks[2], chunks[3]});
        
        Assert.That(chunks[0].GetBlock(0,0,0).name, Is.Not.EqualTo("oak"));
        chunks[0].SetBlock(0, 0, 3, "metal");
        
        this.regionStorage.SaveChunks(chunks.ToList());
        
        Chunk chunk = new Chunk(positions[0], chunkManagerEmpty, new WorldFlatGeneration(), this.regionStorage);
        regionStorage.LoadChunk(chunk);
        FieldInfo fi = typeof(Chunk).GetField("chunkData",    BindingFlags.NonPublic | BindingFlags.Instance)!;
        IChunkData chunkData = (IChunkData)fi.GetValue(chunk)!;
        BlockData[,,] blocks = chunkData.GetBlocks();
        Block block =BlockFactory.GetInstance().BuildFromBlockData(new Vector3D<int>(0,0,3), blocks[0,0,3]);
        Assert.That(block.name, Is.EqualTo("metal"));  
        
        
        Chunk chunk2 = new Chunk(positions[1], chunkManagerEmpty, new WorldFlatGeneration(), this.regionStorage);
        regionStorage.LoadChunk(chunk2);
        chunkData = (IChunkData)fi.GetValue(chunk2)!;
        blocks = chunkData.GetBlocks();
        Block block2 =BlockFactory.GetInstance().BuildFromBlockData(new Vector3D<int>(14,0,3), blocks[14,0,3]); 
        Assert.That(block2.name, Is.EqualTo("oak"));
        
    }  

    [Test]
    public void testOverflowOfChunkReservedSpace() {
        RegionStorage regionStorage = new RegionStorage("./Worlds/newWorld");
        Span<Vector3D<int>> positions = stackalloc Vector3D<int>[]
        {
            new(0, 5 * Chunk.CHUNK_SIZE, 0),
            new(Chunk.CHUNK_SIZE, 0, 0),
            new(2 * Chunk.CHUNK_SIZE, 0, 0)
        };
        Chunk[] chunks = new[]
        {
            getBlockGeneratedChunk(positions[0]),
            getBlockGeneratedChunk(positions[1])
        };
        chunks[1].SetBlock(14, 0, 3, "oak");
        this.regionStorage.SaveChunk(chunks[0]);
        this.regionStorage.SaveChunk(chunks[1]);
        //region file
        // header
        // ---
        // chunks
        // 0,5,0 -> empty chunk, size 1 block
        // 1,0,0 -> chunk with terrain and oak at 14,0,3, size 2 block
        
        // on modifie le chunk 0,5,0 pour qu'il soit plein de block différent et qu'il fasse plus de 1 block
        chunks[0].SetBlock(0, 0, 0, "dirt");
        chunks[0].SetBlock(0, 0, 1, "metal");
        chunks[0].SetBlock(0, 0, 2, "foliage");
        chunks[0].SetBlock(0, 0, 3, "oak");
        
        this.regionStorage.SaveChunk(chunks[0]);
        
        Chunk chunk = new Chunk(positions[1], chunkManagerEmpty, new WorldFlatGeneration(), this.regionStorage);
    } 

    
    
    private Chunk getBlockGeneratedChunk() => getBlockGeneratedChunk(Vector3D<int>.Zero);
    private Chunk getBlockGeneratedChunk(Vector3D<int> position) {
        return ChunkManagerTools.GetBlockGeneratedChunk(chunkManagerEmpty, chunkLoader, position);
    }
}