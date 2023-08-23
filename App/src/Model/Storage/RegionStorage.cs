using System.Text;
using LightningDB;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Logger;
using MinecraftCloneSilk.Model.NChunk;
using NUnit.Framework;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Storage;



public class RegionStorage : IChunkStorage, IDisposable
{

    
    LightningEnvironment env;
    LightningDatabase db;
    private const string dbName = "world";

    public RegionStorage(string pathToChunkFolder) {
        var directory = Directory.CreateDirectory(pathToChunkFolder);
        if(!directory.Exists) {
            throw new Exception("Can't create directory for chunk storage");
        }
        EnvironmentConfiguration envConf = new EnvironmentConfiguration();
        envConf.MaxDatabases = 1;
        envConf.MapSize = 1L * 1024L * 1024L * 1024L; // 1 GiB Gb * Mb * Kb * b
        env = new LightningEnvironment(pathToChunkFolder, envConf);
        env.Open();
        using var tx = env.BeginTransaction();
        db = tx.OpenDatabase(dbName,configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
        tx.Put(db, "version"u8, "1"u8);
        tx.Commit();
    }

    public void SaveChunk(Chunk chunk) {
        SaveChunks(new (){chunk});
    }

    [Logger.Timer]
    public void SaveChunks(List<Chunk> chunks) {
        using var tx = env.BeginTransaction();
        Span<byte> mykey = stackalloc byte[12];
        foreach (Chunk chunk in chunks) {
            if(GetChunkStateInStorage(tx, chunk.position) > chunk.chunkState) continue;
            MathHelper.EncodeVector3Int(mykey, chunk.position.X, chunk.position.Y, chunk.position.Z); 
            MemoryStream stream = new MemoryStream();
            ChunkStorage.SaveChunk(stream, chunk);
            MDBResultCode resultCode = tx.Put(db, mykey, stream.ToArray());
            if(resultCode != MDBResultCode.Success) {
                throw new Exception("Can't save chunk" + resultCode);
            }
        }
        tx.Commit();
    }

    [Logger.Timer] 
    public void LoadChunk(Chunk chunk) {
        Span<byte> mykey = stackalloc byte[12];
        MathHelper.EncodeVector3Int(mykey, chunk.position.X, chunk.position.Y, chunk.position.Z);
        using var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        var (resultCode, key, value) = tx.Get(db, mykey);
        if(resultCode != MDBResultCode.Success) throw new Exception("Can't load chunk" + resultCode);
        ChunkStorage.LoadBlocks(new MemoryStream(value.CopyToNewArray()), chunk);
    }

    public bool isChunkExistInMemory(Vector3D<int> position) {
        Span<byte> mykey = stackalloc byte[12];
        MathHelper.EncodeVector3Int(mykey, position.X, position.Y, position.Z);
        using var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        return tx.ContainsKey(db,mykey);
    }

    public void LoadChunks(List<Chunk> chunks) {
        if(chunks.Count == 0) return;
        foreach (Chunk chunk in chunks) {
            LoadChunk(chunk);
        }
    }

    public ChunkState GetChunkStateInStorage(Vector3D<int> position) {
        using var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        return GetChunkStateInStorage(tx, position);
    }


    private ChunkState GetChunkStateInStorage(LightningTransaction tx, Vector3D<int> position) {
        Span<byte> mykey = stackalloc byte[12];
        MathHelper.EncodeVector3Int(mykey, position.X, position.Y, position.Z);
        var (resultCode, key, value) = tx.Get(db, mykey);
        if(resultCode != MDBResultCode.Success) return ChunkState.EMPTY;
        return ChunkStorage.GetChunkStateInStorage(new MemoryStream(value.CopyToNewArray()));
    }
    
    public void Clear() {
        using var tx = env.BeginTransaction();
        tx.DropDatabase(db);
        tx.Commit();
    }

    public void Dispose() {
        this.Dispose(true);
        GC.SuppressFinalize((object) this);
    }
    
    private void Dispose(bool disposing)
    {
        if (!disposing)
            throw new InvalidOperationException("The LightningEnvironment was not disposed and cannot be reliably dealt with from the finalizer");
        env.Dispose();
    }
    ~RegionStorage() => this.Dispose(false);

}