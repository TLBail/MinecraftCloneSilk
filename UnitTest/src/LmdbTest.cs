using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using Silk.NET.Maths;
using UnitTest.fakeClass;

namespace UnitTest;

using System.Text;
using LightningDB;
public class LmdbTest
{

    public const string pathToFolder = "./Worlds/newWorld";
    [OneTimeSetUp]
    public void setUp() {
        Directory.SetCurrentDirectory("./../../../../");
        
    }
    [SetUp]
    public void setup() {
        DirectoryInfo directory = Directory.CreateDirectory("./Worlds/newWorld");
        foreach (var file in Directory.GetFiles("./Worlds/newWorld")) {
            File.Delete(file);
        }   
    }

    [Test]
    public void simpleTest() {
        EnvironmentConfiguration envConf = new EnvironmentConfiguration();
        envConf.MaxDatabases = 1000;
        using var env = new LightningEnvironment(pathToFolder, envConf);
        env.Open();
        
        Vector3D<int> position = new Vector3D<int>(15, Int32.MaxValue, Int32.MinValue);
        Span<byte> mykey = stackalloc byte[12];
        MathHelper.EncodeVector3Int(mykey, position.X, position.Y, position.Z);
        
        
        MemoryStream stream = new MemoryStream();

        while (stream.Length < 1000) {
            stream.Write("salut"u8.ToArray());
        }
        

        using (var tx = env.BeginTransaction())
        using (var db = tx.OpenDatabase("prout", configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
        {
            tx.Put(db, mykey, stream.ToArray());
            tx.Commit();
        }
        
        using (var tx = env.BeginTransaction())
        using (var db = tx.OpenDatabase("bob", configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create }))
        {
            tx.Put(db, mykey, stream.ToArray());
            tx.Commit();
        }
        
        
        using (var tx = env.BeginTransaction(TransactionBeginFlags.ReadOnly))
        using (var db = tx.OpenDatabase("prout"))
        {
            var (resultCode, key, value) = tx.Get(db, mykey);
            Assert.That(resultCode, Is.EqualTo(MDBResultCode.Success));
            Console.WriteLine("result code: " + resultCode);
            Console.WriteLine($"{MathHelper.DecodeVector3Int(key.AsSpan())} {Encoding.UTF8.GetString(value.AsSpan())}");
            Assert.That(stream.ToArray(), Is.EqualTo(value.CopyToNewArray()));
        }
    }
}