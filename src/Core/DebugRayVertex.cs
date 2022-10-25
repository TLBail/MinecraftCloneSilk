using Silk.NET.Maths;

namespace MinecraftCloneSilk.Core;
public struct DebugRayVertex
{
    public Vector3D<float> position;
    public Vector3D<float> color;

    public DebugRayVertex(Vector3D<float> position, Vector3D<float> color)
    {
        this.position = position;
        this.color = color;
    }
}
