using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model
{
    public struct CubeVertex
    {
        public Vector4D<float> position; // 4 * 4 
        public Vector2D<float> texCoords; // 2 * 4 offset 16
        public int ambientOcclusion; // 4 offset 24
        public int lightLevel; // 4 offset 28
        public CubeVertex(Vector3D<float> position, Vector2D<float> texCoords, int ambientOcclusion = 0, int lightLevel = 0)
        {
            this.position = new Vector4D<float>(position.X, position.Y, position.Z, 1.0f);
            this.texCoords = new Vector2D<float>(texCoords.X, texCoords.Y);
            this.ambientOcclusion = ambientOcclusion;
            this.lightLevel = lightLevel;
        }

    }
}
