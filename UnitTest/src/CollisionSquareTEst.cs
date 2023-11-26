using System.Numerics;
using MinecraftCloneSilk.Collision;
using Silk.NET.Maths;
using Plane = MinecraftCloneSilk.Collision.Plane;

namespace UnitTest;

[TestFixture]
public class CollisionSquareTEst
{
    
    [Test]
    public void TestCollision()
    {
        Vector3 A = new Vector3(0.0f, 0.5f, -0.5f);
        Vector3 B = new Vector3(0.0f, 0.5f, 0.5f);
        Vector3 C = new Vector3(0.0f, -0.5f, 0.5f);
        Vector3 D = new Vector3(0.0f, -0.5f, -0.5f);
        Square square = new Square(A,  B, C, D, Vector3.Zero);


        Ray ray = new Ray(
            new Vector3(-5f, 0, 0), 
            new Vector3(1, 0, 0));

        Plane plane = new Plane(A, B , D, Vector3.Zero);

        Assert.IsTrue(plane.Intersect(ray).haveHited);
        Assert.IsTrue(square.Intersect(ray));
        
    }

    [Test]
    public void TestNotCollingRay()
    {
        Vector3 A = new Vector3(0.0f, 0.5f, -0.5f);
        Vector3 B = new Vector3(0.0f, 0.5f, 0.5f);
        Vector3 C = new Vector3(0.0f, -0.5f, 0.5f);
        Vector3 D = new Vector3(0.0f, -0.5f, -0.5f);
        Square square = new Square(A,  B, C, D, Vector3.Zero);


        Ray ray = new Ray(
            new Vector3(-5f, 0f, 0), 
            new Vector3(-1, 0, 0));

        Plane plane = new Plane(A, B , D, Vector3.Zero);

        Assert.IsFalse(plane.Intersect(ray).haveHited);
        Assert.IsFalse(square.Intersect(ray));

    }
    
    [Test]
    public void TestNotCollingRayInPlaneButNotInSquare()
    {
        Vector3 A = new Vector3(0.0f, 0.5f, -0.5f);
        Vector3 B = new Vector3(0.0f, 0.5f, 0.5f);
        Vector3 C = new Vector3(0.0f, -0.5f, 0.5f);
        Vector3 D = new Vector3(0.0f, -0.5f, -0.5f);
        Square square = new Square(A,  B, C, D, Vector3.Zero);


        Ray ray = new Ray(
            new Vector3(-5f, 2f, 0), 
            new Vector3(1, 0, 0));

        Plane plane = new Plane(A, B , D, Vector3.Zero);

        Assert.IsTrue(plane.Intersect(ray).haveHited);
        Assert.IsFalse(square.Intersect(ray));

    }
    [Test]
    public void TestLeftFace()
    {
        Vector3 position = Vector3.Zero;

        Vector3 A = new Vector3(position.X - 0.5f, position.Y - 0.5f, position.Z + 0.5f);
        Vector3 B = new Vector3(position.X - 0.5f, position.Y + 0.5f, position.Z + 0.5f);
        Vector3 C = new Vector3(position.X - 0.5f, position.Y + 0.5f, position.Z - 0.5f);
        Vector3 D = new Vector3(position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f);
        Square square = new Square(
            D,C,B,A,
            new Vector3(position.X - 0.5f, position.Y, position.Z)
        );

        
        

        Ray ray = new Ray(
            new Vector3(-5f, 0f, 0), 
            new Vector3(1, 0, 0));

        Plane plane = new Plane(D,C,A, new Vector3(position.X - 0.5f, position.Y, position.Z));

        Assert.IsTrue(plane.Intersect(ray).haveHited);
        Assert.IsTrue(square.Intersect(ray));

    }
    
    
    [Test]
    public void TestLeftFaceOld()
    {
        
        Vector3 position = Vector3.Zero;

        Ray ray = new Ray(
            new Vector3(-5f, 0f, 0), 
            new Vector3(1, 0, 0));

        Plane plane = new Plane(new Vector3(position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
            new Vector3(position.X - 0.5f, position.Y + 0.5f, position.Z - 0.5f),
            new Vector3(position.X - 0.5f, position.Y - 0.5f, position.Z + 0.5f)
            , new Vector3(position.X - 0.5f, position.Y, position.Z));

        Assert.IsTrue(plane.Intersect(ray).haveHited);
    }
    
    
    
}