using System.Numerics;
using Assimp;

namespace MinecraftCloneSilk.ModelLoading;

public struct Vertex
{
    public Vector3 position;
    public Vector3 normal;
    public Vector2 texCoords;
    
    public Vertex(Vector3 position, Vector3 normal, Vector2 texCoords) {
        this.position = position;
        this.normal = normal;
        this.texCoords = texCoords;
    }
    
    public Vertex(Vector3D position, Vector3D normal, Vector2D texCoords) {
        this.position = new Vector3(position.X, position.Y, position.Z);
        this.normal = new Vector3(normal.X, normal.Y, normal.Z);
        this.texCoords = new Vector2(texCoords.X, texCoords.Y);
    }
}