using BenchmarkDotNet.Attributes;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;

namespace Benchmark.BenchMarks;

public class ChunkFaceBenchmark
{
    private BlockFactory blockFactory;
    private BlockData[,,] blocks;
    [GlobalSetup]
    public void setup() {
        Directory.SetCurrentDirectory("./../../../../../../../../");
        blockFactory = BlockFactory.GetInstance();
        blocks = new BlockData[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];
        blocks[2, 0, 2] = new BlockData { id = 1 };
        blocks[0, 0, 0] = new BlockData { id = blockFactory.GetBlockIdByName("foliage") };
    }

    [Benchmark]
    public void benchmark() {
        ChunkFaceUtils.GetChunkFaceFlags(blockFactory, blocks);
    }
    
    
}