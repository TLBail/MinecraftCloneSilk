using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace UnitTest.fakeClass;

public class ChunkProviderEmpty : ChunkProvider
{
    public Dictionary<Vector3D<int>, Chunk> chunks = new Dictionary<Vector3D<int>, Chunk>();

    public WorldGenerator worldGenerator;

    public ChunkProviderEmpty(WorldGenerator worldGenerator) {
        this.worldGenerator = worldGenerator;
    }

    public Chunk getChunk(Vector3D<int> position) {
        if (chunks.ContainsKey(position)) {
            return chunks[position];
        }    
        chunks.Add(position, new Chunk(position, this, worldGenerator));
        return chunks[position];
    }
}