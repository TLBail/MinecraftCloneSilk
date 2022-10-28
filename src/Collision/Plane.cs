using Silk.NET.Maths;

namespace MinecraftCloneSilk.Collision;

public class Plane
{
    private Vector3D<float> planeNormal;
    private Vector3D<float> planeCenter;

    public Plane(
        Vector3D<float> point1,
        Vector3D<float> point2,
        Vector3D<float> point3,
        Vector3D<float> center)
    {
        planeNormal = Vector3D.Cross(Vector3D.Subtract(point2, point1),
            Vector3D.Subtract(point3, point1));
        planeNormal = Vector3D.Normalize(planeNormal);
        this.planeCenter = center;
    }

    public bool intersect(Ray ray, float t)
    {
        float denom = Vector3D.Dot(planeNormal, ray.dir);
        if (denom > 1.0e-6) {
            Vector3D<float> p010 = Vector3D.Subtract(planeCenter, ray.orig );
            t = Vector3D.Dot(p010, planeNormal) / denom;
            return (t >= 0);
        }

        return false;
    }
}