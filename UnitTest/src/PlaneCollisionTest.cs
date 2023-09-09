using System.Numerics;
using MinecraftCloneSilk.Collision;
using Silk.NET.Maths;
using Plane = MinecraftCloneSilk.Collision.Plane;

namespace UnitTest;

public class PlaneCollisionTest
{
    [Test]
    public void Test1()
    {
        Assert.IsTrue(true);
        Ray ray = new Ray(Vector3.Zero, new Vector3(1.0f, 0.0f, 0.0f));
        Plane plane = new Plane(
            new Vector3(0.0f, 0.5f, 0.5f),
            new Vector3(0.0f, -0.5f, 0.5f),
            new Vector3(0.0f, -0.5f, -0.5f),
            new Vector3(0f, 0f, 0f)
        );
        float distance = 10;
        Assert.IsTrue(plane.Intersect(ray, ref distance));
        
        Ray rayThatDontIntersect = new Ray(Vector3.Zero, new Vector3(-1.0f, 0.0f, 0.0f));
        Assert.IsFalse(plane.Intersect(rayThatDontIntersect, ref distance));
        
        
        Plane planeInverse = new Plane(
            new Vector3(0.0f, -0.5f, -0.5f),
            new Vector3(0.0f, -0.5f, 0.5f),
            new Vector3(0.0f, 0.5f, 0.5f),
            new Vector3(0f, 0f, 0f)
        );
        
        Assert.IsFalse(planeInverse.Intersect(ray, ref distance));

    }
}