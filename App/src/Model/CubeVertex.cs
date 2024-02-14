using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model
{
    public struct CubeVertex
    {
        public int position; 
        /*
         *                                          ao
         *     0000 0000 0000 0000 0000 0000 0000 0000
         *                                     (2  )()
         * 2 => light level
         */
        public int data; //0x0 : 0x3 ao , 0x4 : 0x7 id
        public Vector2 texCoords; // 2 * 4 offset 16
        
        public int GetAmbientOcclusion() {
            return data & 0x3;
        }
        public int GetLightLevel() {
            return (data >> 2) & 0xF;
        }
        public void SetAmbientOcclusion(int ao) {
            data = (data & ~0x3) | ao;
        }
        
        public void SetLightLevel(int lightLevel) {
            data = (data & ~(0xF << 2)) | (lightLevel << 2);
        }
    }
}
