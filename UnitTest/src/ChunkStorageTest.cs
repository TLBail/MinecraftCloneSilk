using MinecraftCloneSilk.Model.Storage;

namespace UnitTest;

public class ChunkStorageTest
{
    [Test]
    public void testChunkStorageHaveOpenFolder() {
        ChunkStorage chunkStorage = new ChunkStorage("./Worlds/newWorld");
        Assert.IsNotNull(chunkStorage);
    }
}