using MinecraftCloneSilk.Collision;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Collision;

public class AABBCube
{
    public Vector3D<float>[] bounds = new Vector3D<float>[2];

    public AABBCube(Vector3D<float> min, Vector3D<float> max)
    {
        bounds[0] = min;
        bounds[1] = max;
    }
    
    
    public bool Intersect(Ray r, float t)
    {
        float tmin, tmax, tymin, tymax, tzmin, tzmax;

        tmin = (bounds[r.sign[0]].X - r.orig.X) * r.invdir.X;
        tmax = (bounds[1 - r.sign[0]].X - r.orig.X) * r.invdir.X;
        tymin = (bounds[r.sign[1]].Y - r.orig.Y) * r.invdir.Y;
        tymax = (bounds[1 - r.sign[1]].Y - r.orig.Y) * r.invdir.Y;

        if ((tmin > tymax) || (tymin > tmax))
            return false;

        if (tymin > tmin)
            tmin = tymin;
        if (tymax < tmax)
            tmax = tymax;

        tzmin = (bounds[r.sign[2]].Z - r.orig.Z) * r.invdir.Z;
        tzmax = (bounds[1 - r.sign[2]].Z - r.orig.Z) * r.invdir.Z;

        if ((tmin > tzmax) || (tzmin > tmax))
            return false;

        if (tzmin > tmin)
            tmin = tzmin;
        if (tzmax < tmax)
            tmax = tzmax;

        t = tmin;

        if (t < 0) {
            t = tmax;
            if (t < 0) return false;
        }

        return true;
    }

    public HitInfo Intersect(Ray r)
    {
        float tmin, tmax, tymin, tymax, tzmin, tzmax;

        tmin = (bounds[r.sign[0]].X - r.orig.X) * r.invdir.X;
        tmax = (bounds[1 - r.sign[0]].X - r.orig.X) * r.invdir.X;
        tymin = (bounds[r.sign[1]].Y - r.orig.Y) * r.invdir.Y;
        tymax = (bounds[1 - r.sign[1]].Y - r.orig.Y) * r.invdir.Y;

        if ((tmin > tymax) || (tymin > tmax))
            return new HitInfo(false, 0);

        if (tymin > tmin)
            tmin = tymin;
        if (tymax < tmax)
            tmax = tymax;

        tzmin = (bounds[r.sign[2]].Z - r.orig.Z) * r.invdir.Z;
        tzmax = (bounds[1 - r.sign[2]].Z - r.orig.Z) * r.invdir.Z;

        if ((tmin > tzmax) || (tzmin > tmax))
            return new HitInfo(false, 0);

        if (tzmin > tmin)
            tmin = tzmin;
        if (tzmax < tmax)
            tmax = tzmax;

        float t = tmin;

        if (t < 0) {
            t = tmax;
            if (t < 0) return new HitInfo(false, 0);
        }

        return new HitInfo(true,t);
    }
}