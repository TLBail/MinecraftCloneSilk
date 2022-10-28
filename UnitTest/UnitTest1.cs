using MinecraftCloneSilk.Collision;
using Silk.NET.Maths;
using Plane = MinecraftCloneSilk.Collision.Plane;

namespace UnitTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Assert.IsTrue(true);
        Ray ray = new Ray(Vector3D<float>.Zero, new Vector3D<float>(1.0f, 0.0f, 0.0f));
        Plane plane = new Plane(
            new Vector3D<float>(0.0f, 0.5f, 0.5f),
            new Vector3D<float>(0.0f, -0.5f, 0.5f),
            new Vector3D<float>(0.0f, -0.5f, -0.5f),
            new Vector3D<float>(0f, 0f, 0f)
        );
        Assert.IsTrue(plane.intersect(ray, 10));
        
        Ray rayThatDontIntersect = new Ray(Vector3D<float>.Zero, new Vector3D<float>(-1.0f, 0.0f, 0.0f));
        Assert.IsFalse(plane.intersect(rayThatDontIntersect, 10));
        
    }
}