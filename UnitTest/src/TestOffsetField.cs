using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MinecraftCloneSilk.Model;

namespace UnitTest;

[TestFixture]
public class TestOffsetField
{
    [Test]
    public unsafe void testCalculatingOffset() {
        CubeVertex vertex = new CubeVertex();
        var offset = new IntPtr(&vertex) - new IntPtr(&vertex.position);
        Assert.IsTrue(offset == 0);

        offset = new IntPtr(&vertex.data) - new IntPtr(&vertex);
        Assert.That(Marshal.OffsetOf(vertex.GetType(), "data"), Is.EqualTo(offset));

        int offset2 = Unsafe.ByteOffset(ref Unsafe.As<CubeVertex, byte>(ref vertex),
            ref Unsafe.As<int, byte>(ref vertex.data)).ToInt32();
        Console.WriteLine("byteoffset" + offset2);
        Assert.That((int)offset, Is.EqualTo(offset2));

        int offset3 = GetOffset(ref vertex, ref vertex.data);
        Console.WriteLine("byteoffset" + offset3);

        int offset4 = GetOffset(ref vertex, ref vertex.position);
        Assert.That(offset4, Is.EqualTo(0));
        
        int offset5 = GetOffset(ref vertex, ref vertex.texCoords);
        Assert.That(offset5, Is.EqualTo(8));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetOffset<T, F>(ref T obj,ref F field) where T : struct {
        return Unsafe.ByteOffset(ref Unsafe.As<T, byte>(ref obj), ref Unsafe.As<F, byte>(ref field)).ToInt32();
    }
    
}