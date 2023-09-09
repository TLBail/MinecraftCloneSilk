using System.Numerics;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Collision;

public class Plane
{
    public Vector3 planeNormal { get; private set; }
    public Vector3 planeCenter { get; private set; }
    private float distance = 0;

    public Plane(
        Vector3 a,
        Vector3 b,
        Vector3 d,
        Vector3 center)
    {
        planeNormal = Vector3.Cross(Vector3.Subtract(b, a),
            Vector3.Subtract(d, a));
        planeNormal = Vector3.Normalize(planeNormal);
        this.planeCenter = center;
        distance = CalculateDistanceToOrigin(planeNormal, planeCenter);
    }
    
    public Plane(Vector3 planeNormal, Vector3 planeCenter)
    {
        this.planeNormal = planeNormal;
        this.planeCenter = planeCenter;
        distance = CalculateDistanceToOrigin(planeNormal, planeCenter);
    }
    public float CalculateDistanceToOrigin(Vector3 normal, Vector3 pointOnPlane)
    {
        return Vector3.Dot(normal, pointOnPlane);
    }

    
    public float GetSignedDistanceToPlane(Vector3 point)
    {
        return Vector3.Dot(planeNormal, point) - distance;
    }
    

    public bool Intersect(Ray ray, ref float t)
    {
        float denom = Vector3.Dot(planeNormal, ray.dir);
        if (denom > 1.0e-6) {
            Vector3 p010 = Vector3.Subtract(planeCenter, ray.orig );
            t = Vector3.Dot(p010, planeNormal) / denom;
            return (t >= 0);
        }

        return false;
    }

    public HitInfo Intersect(Ray ray)
    {
        float denom = Vector3.Dot(planeNormal, ray.dir);
        if (denom > 1.0e-6) {
            Vector3 p010 = Vector3.Subtract(planeCenter, ray.orig );
            float t = Vector3.Dot(p010, planeNormal) / denom;
            return new HitInfo(t >= 0, t) ;
        }

        return new HitInfo(false, 0);
    }
}