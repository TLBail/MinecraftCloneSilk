using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model
{
    public struct CubeVertex
    {
        public Vector4D<float> position;
        public Vector2D<float> texCoords;
        public int ambientOcclusion;
        public int lightLevel;

        public CubeVertex(Vector3D<float> position, Vector2D<float> texCoords, int ambientOcclusion = 0, int lightLevel = 0)
        {
            this.position = new Vector4D<float>(position.X, position.Y, position.Z, 1.0f);
            this.texCoords = new Vector2D<float>(texCoords.X, texCoords.Y);
            this.ambientOcclusion = ambientOcclusion;
            this.lightLevel = lightLevel;
        }

    }
}
