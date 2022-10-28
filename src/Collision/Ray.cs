using System.Numerics;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Collision;

public class Ray
{
    public Vector3D<float> orig;
    public Vector3D<float> dir;
    public Vector3D<float> invdir;
    
    public int[] sign = new int[3];


    public Ray(Vector3D<float> orig, Vector3D<float> dir)
    {
        this.orig = orig;
        this.dir = dir;

        invdir = new Vector3D<float>(
            1.0f / dir.X,
            1.0f / dir.Y,
            1.0f  / dir.Z
            );
        sign[0] = (invdir.X < 0) ? 1: 0;
        sign[1] = (invdir.Y < 0) ? 1: 0;
        sign[2] = (invdir.Z < 0) ? 1: 0;

    }

    public Vector3D<int> projectToBlock(float offset)
    {
        Vector3D<float> projetion = orig + Vector3D.Multiply(dir, offset);
        return new Vector3D<int>(
            (int)Math.Round(projetion.X, 0), 
            (int)Math.Round(projetion.Y, 0),
            (int)Math.Round(projetion.Z, 0)
            );
    }
}