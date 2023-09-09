using System.Numerics;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Collision;

public class Ray
{
    public Vector3 orig;
    public Vector3 dir;
    public Vector3 invdir;
    
    public int[] sign = new int[3];


    public Ray(Vector3 orig, Vector3 dir)
    {
        this.orig = orig;
        this.dir = dir;

        invdir = new Vector3(
            1.0f / dir.X,
            1.0f / dir.Y,
            1.0f  / dir.Z
            );
        sign[0] = (invdir.X < 0) ? 1: 0;
        sign[1] = (invdir.Y < 0) ? 1: 0;
        sign[2] = (invdir.Z < 0) ? 1: 0;

    }

    
    public List<Vector3> ProjetedCoords(float size)
    {
        List<Vector3> hitedPosition = new List<Vector3>();
        for (float i = 0; i < size; i += 0.1f) {
            Vector3 projetion = orig + Vector3.Multiply(dir, i);
            hitedPosition.Add( new Vector3(
                (int)Math.Round(projetion.X, 0), 
                (int)Math.Round(projetion.Y, 0),
                (int)Math.Round(projetion.Z, 0)
            ));
            
            
            projetion = orig + Vector3.Multiply(dir, i) + new Vector3(0.1f, 0, 0);
            hitedPosition.Add( new Vector3(
                (int)Math.Round(projetion.X, 0), 
                (int)Math.Round(projetion.Y, 0),
                (int)Math.Round(projetion.Z, 0)
            ));
            
            projetion = orig + Vector3.Multiply(dir, i) + new Vector3(0, 0.1f, 0);
            hitedPosition.Add( new Vector3(
                (int)Math.Round(projetion.X, 0), 
                (int)Math.Round(projetion.Y, 0),
                (int)Math.Round(projetion.Z, 0)
            ));
            
            projetion = orig + Vector3.Multiply(dir, i) + new Vector3(0.1f, 0.1f, 0);
            hitedPosition.Add( new Vector3(
                (int)Math.Round(projetion.X, 0), 
                (int)Math.Round(projetion.Y, 0),
                (int)Math.Round(projetion.Z, 0)
            ));
            
            
            
            projetion = orig + Vector3.Multiply(dir, i) + new Vector3(0, 0, 0.1f);
            hitedPosition.Add( new Vector3(
                (int)Math.Round(projetion.X, 0), 
                (int)Math.Round(projetion.Y, 0),
                (int)Math.Round(projetion.Z, 0)
            ));
            
            
            projetion = orig + Vector3.Multiply(dir, i) + new Vector3(0.1f, 0, 0.1f);
            hitedPosition.Add( new Vector3(
                (int)Math.Round(projetion.X, 0), 
                (int)Math.Round(projetion.Y, 0),
                (int)Math.Round(projetion.Z, 0)
            ));
            
            
            projetion = orig + Vector3.Multiply(dir, i) + new Vector3(0, 0.1f, 0.1f);
            hitedPosition.Add( new Vector3(
                (int)Math.Round(projetion.X, 0), 
                (int)Math.Round(projetion.Y, 0),
                (int)Math.Round(projetion.Z, 0)
            ));

            projetion = orig + Vector3.Multiply(dir, i) + new Vector3(0.1f, 0.1f, 0.1f);
            hitedPosition.Add( new Vector3(
                (int)Math.Round(projetion.X, 0), 
                (int)Math.Round(projetion.Y, 0),
                (int)Math.Round(projetion.Z, 0)
            ));

        }

        return hitedPosition;
    }
    
    public Vector3 ProjectToBlock(float offset)
    {
        Vector3 projetion = orig + Vector3.Multiply(dir, offset);
        return new Vector3(
            (int)Math.Round(projetion.X, 0), 
            (int)Math.Round(projetion.Y, 0),
            (int)Math.Round(projetion.Z, 0)
            );
    }
}