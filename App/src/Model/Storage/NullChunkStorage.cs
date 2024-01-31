using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;


namespace MinecraftCloneSilk.Model.Storage;

public class NullChunkStorage : IChunkStorage
{
    public ChunkState GetChunkStateInStorage(Vector3D<int> position) => ChunkState.EMPTY;
    
    public void SaveChunkAsync(Chunk chunk) { chunk.blockModified = false; }
    public void SaveChunk(Chunk chunk) { chunk.blockModified = false; }
    public void SaveChunks(List<Chunk> chunks) { chunks.ForEach(chunk => SaveChunk(chunk));}
    public bool IsChunkExistInMemory(Vector3D<int> position) { return false;}
    public void LoadChunk(Chunk chunk) { }
    public void LoadChunks(List<Chunk> chunks) { }
    public void Dispose() { }
}