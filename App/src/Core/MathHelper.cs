using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;


namespace MinecraftCloneSilk.Core
{
    public static class MathHelper
    {
        public static float DegreesToRadians(float degrees)
        {
            return MathF.PI / 180f * degrees;
        }
        
        public static void EncodeVector3Int(Span<byte> vector, int a, int b, int c)
        {
            var aBytes = BitConverter.GetBytes(a);
            var bBytes = BitConverter.GetBytes(b);
            var cBytes = BitConverter.GetBytes(c);
            aBytes.CopyTo(vector);
            bBytes.CopyTo(vector[4..]);
            cBytes.CopyTo(vector[8..]);
        }
        public static Vector3D<int> DecodeVector3Int(ReadOnlySpan<byte> bytes) {
            return new(BitConverter.ToInt32(bytes), BitConverter.ToInt32(bytes[4..]), BitConverter.ToInt32(bytes[8..]));
        }
    }
}
