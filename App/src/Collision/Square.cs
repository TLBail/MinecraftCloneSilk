using System.Numerics;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Collision;

public class Square
{
    private Plane plane;

    private Vector3D<float> point1;
    private Vector3D<float> point2;
    private Vector3D<float> point3;
    private Vector3D<float> point4;
    public Square(Vector3D<float> point1,
        Vector3D<float> point2,
        Vector3D<float> point3,
        Vector3D<float> point4,
        Vector3D<float> center)
    {
        plane = new Plane(point1, point2, point4, center);
        this.point1 = point1;
        this.point2 = point2;
        this.point3 = point3;
        this.point4 = point4;
    }

    public bool Intersect(Ray ray)
    {
        HitInfo hitInfo = plane.Intersect(ray);
        if (hitInfo.haveHited == false) return false;
        float t = hitInfo.fNorm;
        
        
        Vector3D<float> intersectPoint = ray.orig + (t * ray.dir);
        float o1 = Orient(intersectPoint, point1, point2, plane.planeNormal);
        float o2 = Orient(intersectPoint, point2, point3, plane.planeNormal);
        float o3 = Orient(intersectPoint, point3, point4, plane.planeNormal);
        float o4 = Orient(intersectPoint, point4, point1, plane.planeNormal);
        return (o1 >= 0 && o2 >= 0 && o3 >= 0 && o4 >= 0) ||
               (o1 <= 0 && o2 <= 0 && o3 <= 0 && o4 <= 0);
    }
    
    public HitInfo IntersectInfo(Ray ray)
    {
        HitInfo hitInfo = plane.Intersect(ray);
        if (hitInfo.haveHited == false) return new HitInfo(false, hitInfo.fNorm) ;
        float t = hitInfo.fNorm;
        
        
        Vector3D<float> intersectPoint = ray.orig + (t * ray.dir);
        float o1 = Orient(intersectPoint, point1, point2, plane.planeNormal);
        float o2 = Orient(intersectPoint, point2, point3, plane.planeNormal);
        float o3 = Orient(intersectPoint, point3, point4, plane.planeNormal);
        float o4 = Orient(intersectPoint, point4, point1, plane.planeNormal);
        return new HitInfo( (o1 >= 0 && o2 >= 0 && o3 >= 0 && o4 >= 0) ||
               (o1 <= 0 && o2 <= 0 && o3 <= 0 && o4 <= 0), hitInfo.fNorm);
    }

    private float Orient(Vector3D<float> a, Vector3D<float> b, Vector3D<float> c, Vector3D<float> n)
    {
        return Vector3D.Dot(Vector3D.Cross((b - a), (c - a)), n);
    }

}