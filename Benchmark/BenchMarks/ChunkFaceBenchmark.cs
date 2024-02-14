using BenchmarkDotNet.Attributes;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;

namespace Benchmark.BenchMarks;

public class ChunkFaceBenchmark
{
    private BlockFactory blockFactory = null!;
    private BlockData[,,] blocks = null!;
    [GlobalSetup]
    public void Setup() {
        Directory.SetCurrentDirectory("./../../../../../../../../");
        blockFactory = BlockFactory.GetInstance();
        blocks = new BlockData[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];
        blocks[2, 0, 2] = new BlockData (1 );
        blocks[0, 0, 0] = new BlockData ( blockFactory.GetBlockIdByName("foliage") );
    }

    [Benchmark]
    public void Benchmark() {
        ChunkFaceUtils.GetChunkFaceFlags(blockFactory, blocks);
    }
    
    
}