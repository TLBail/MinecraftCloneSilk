using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using LightningDB;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Logger;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.NChunk;
using NUnit.Framework;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Storage;



public class RegionStorage : IChunkStorage, IDisposable
{
    public void ChunkUnloaderProcessor() {
        foreach (Chunk chunk in chunksToSave.GetConsumingEnumerable()) {
            if (ChunkStateTools.IsChunkIsLoading(chunk.chunkState)) {
                chunk.RemoveRequiredByChunkSaver();
                continue;
            }
            SaveChunk(chunk);
            chunk.RemoveRequiredByChunkSaver();
        }
    }

    
    LightningEnvironment env;
    LightningDatabase db;
    private const string DB_NAME = "world";
    private readonly BlockingCollection<Chunk> chunksToSave = new BlockingCollection<Chunk>();
    private readonly Task chunkUnloaderTask;

    public RegionStorage(string pathToChunkFolder) {
        chunkUnloaderTask = new Task(ChunkUnloaderProcessor);
        chunkUnloaderTask.Start();
        
        var directory = Directory.CreateDirectory(pathToChunkFolder);
        if(!directory.Exists) {
            throw new Exception("Can't create directory for chunk storage");
        }
        EnvironmentConfiguration envConf = new EnvironmentConfiguration();
        envConf.MaxDatabases = 1;
        envConf.MapSize = 1024L * 1024L * 1024L * 1024L; // 1024 GiB Gb * Mb * Kb * b
        env = new LightningEnvironment(pathToChunkFolder, envConf);
        env.Open();
        using var tx = env.BeginTransaction();
        db = tx.OpenDatabase(DB_NAME,configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });
        tx.Put(db, "version"u8, "1"u8);
        tx.Commit();
    }

    public void SaveChunk(Chunk chunk) {
        SaveChunks(new (){chunk});
    }
    
    public void SaveChunkAsync(Chunk chunk) {
        chunk.AddRequiredByChunkSaver();
        chunksToSave.Add(chunk);
    }

    [Logger.Timer]
    public void SaveChunks(List<Chunk> chunks) {
        using var tx = env.BeginTransaction();
        Span<byte> mykey = stackalloc byte[12];
        foreach (Chunk chunk in chunks) {
            //Todo fix this
            //Debug.Assert(GetChunkStateInStorage(tx, chunk.position) == chunk.chunkStateInStorage);
            //Debug.Assert(GetChunkStateInStorage(tx, chunk.position) <= chunk.chunkState, $"erreur : chunkState in storage:{GetChunkStateInStorage(tx, chunk.position)} is higher than chunkState:{chunk.chunkState}");
            Debug.Assert(!ChunkStateTools.IsChunkIsLoading(chunk.chunkState), $"try to save chunk with bad chunk state 💾 : {chunk.chunkState}");
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

    public void LoadChunk(Chunk chunk) {
        Span<byte> mykey = stackalloc byte[12];
        MathHelper.EncodeVector3Int(mykey, chunk.position.X, chunk.position.Y, chunk.position.Z);
        using var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly);
        var (resultCode, key, value) = tx.Get(db, mykey);
        if(resultCode != MDBResultCode.Success) throw new Exception("Can't load chunk" + resultCode);
        ChunkStorage.LoadBlocks(new MemoryStream(value.CopyToNewArray()), chunk);
    }

    public bool IsChunkExistInMemory(Vector3D<int> position) {
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
        chunksToSave.CompleteAdding();
        Console.WriteLine("Waiting for chunk to be saved");
        chunkUnloaderTask.Wait();
        Console.WriteLine("Chunk saved");
        chunkUnloaderTask.Dispose();
        chunksToSave.Dispose();
        env.Dispose();
    }
    ~RegionStorage() => this.Dispose(false);

}