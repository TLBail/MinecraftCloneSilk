using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model
{
    public struct CubeVertex
    {
        public Vector3D<float> position;
        public Vector2D<float> texCoords;

        public CubeVertex(Vector3D<float> position, Vector2D<float> texCoords)
        {
            this.position = position;
            this.texCoords = texCoords;
        }

    }
}
