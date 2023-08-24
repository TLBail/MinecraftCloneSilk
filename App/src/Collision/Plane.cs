using Silk.NET.Maths;

namespace MinecraftCloneSilk.Collision;

public class Plane
{
    public Vector3D<float> planeNormal;
    public Vector3D<float> planeCenter;

    public Plane(
        Vector3D<float> a,
        Vector3D<float> b,
        Vector3D<float> d,
        Vector3D<float> center)
    {
        planeNormal = Vector3D.Cross(Vector3D.Subtract(b, a),
            Vector3D.Subtract(d, a));
        planeNormal = Vector3D.Normalize(planeNormal);
        this.planeCenter = center;
    }

    public bool Intersect(Ray ray, ref float t)
    {
        float denom = Vector3D.Dot(planeNormal, ray.dir);
        if (denom > 1.0e-6) {
            Vector3D<float> p010 = Vector3D.Subtract(planeCenter, ray.orig );
            t = Vector3D.Dot(p010, planeNormal) / denom;
            return (t >= 0);
        }

        return false;
    }

    public HitInfo Intersect(Ray ray)
    {
        float denom = Vector3D.Dot(planeNormal, ray.dir);
        if (denom > 1.0e-6) {
            Vector3D<float> p010 = Vector3D.Subtract(planeCenter, ray.orig );
            float t = Vector3D.Dot(p010, planeNormal) / denom;
            return new HitInfo(t >= 0, t) ;
        }

        return new HitInfo(false, 0);
    }
}