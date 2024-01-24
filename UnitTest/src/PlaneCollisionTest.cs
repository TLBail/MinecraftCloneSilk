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
    
    [Test]
    public void TestDistanceToPlane_PointAbovePlane_ReturnsPositiveDistance()
    {
        // Arrange
        Vector3 normal = new Vector3(0, 1, 0); // Normal pointing up
        Vector3 center = new Vector3(0, 0, 0); // Plane at origin
        Plane plane = new Plane(normal, center);
        Vector3 pointAbove = new Vector3(0, 1, 0); // Point above the plane

        // Act
        float distance = plane.GetSignedDistanceToPlane(pointAbove);

        // Assert
        Assert.Greater(distance, 0, "The distance should be positive for points above the plane.");
    }

    [Test]
    public void TestDistanceToPlane_PointBelowPlane_ReturnsNegativeDistance()
    {
        // Arrange
        Vector3 normal = new Vector3(0, 1, 0); // Normal pointing up
        Vector3 center = new Vector3(0, 0, 0); // Plane at origin
        Plane plane = new Plane(normal, center);
        Vector3 pointBelow = new Vector3(0, -1, 0); // Point below the plane

        // Act
        float distance = plane.GetSignedDistanceToPlane(pointBelow);

        // Assert
        Assert.Less(distance, 0, "The distance should be negative for points below the plane.");
    }

    [Test]
    public void TestDistanceToPlane_PointOnPlane_ReturnsZero()
    {
        // Arrange
        Vector3 normal = new Vector3(0, 1, 0); // Normal pointing up
        Vector3 center = new Vector3(0, 0, 0); // Plane at origin
        Plane plane = new Plane(normal, center);
        Vector3 pointOnPlane = new Vector3(0, 0, 0); // Point on the plane

        // Act
        float distance = plane.GetSignedDistanceToPlane(pointOnPlane);

        // Assert
        Assert.AreEqual(0, distance, "The distance should be zero for points on the plane.");
    }
}