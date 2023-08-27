using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model
{
    public struct CubeVertex
    {
        public Vector4D<float> position;
        public Vector4D<float> texCoords;

        public CubeVertex(Vector3D<float> position, Vector2D<float> texCoords)
        {
            this.position = new Vector4D<float>(position.X, position.Y, position.Z, 1.0f);
            this.texCoords = new Vector4D<float>(texCoords.X, texCoords.Y, 0.0f, 0.0f);
            
        }

    }
}
