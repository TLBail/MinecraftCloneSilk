using System.Numerics;
using MinecraftCloneSilk.Collision;
using Silk.NET.Maths;
using Plane = MinecraftCloneSilk.Collision.Plane;

namespace UnitTest;

public class CollisionSquareTEst
{
    
    [Test]
    public void testCollision()
    {
        Vector3D<float> A = new Vector3D<float>(0.0f, 0.5f, -0.5f);
        Vector3D<float> B = new Vector3D<float>(0.0f, 0.5f, 0.5f);
        Vector3D<float> C = new Vector3D<float>(0.0f, -0.5f, 0.5f);
        Vector3D<float> D = new Vector3D<float>(0.0f, -0.5f, -0.5f);
        Square square = new Square(A,  B, C, D, Vector3D<float>.Zero);


        Ray ray = new Ray(
            new Vector3D<float>(-5f, 0, 0), 
            new Vector3D<float>(1, 0, 0));

        Plane plane = new Plane(A, B , D, Vector3D<float>.Zero);

        Assert.IsTrue(plane.intersect(ray).haveHited);
        Assert.IsTrue(square.intersect(ray));
        
    }

    [Test]
    public void testNotCollingRay()
    {
        Vector3D<float> A = new Vector3D<float>(0.0f, 0.5f, -0.5f);
        Vector3D<float> B = new Vector3D<float>(0.0f, 0.5f, 0.5f);
        Vector3D<float> C = new Vector3D<float>(0.0f, -0.5f, 0.5f);
        Vector3D<float> D = new Vector3D<float>(0.0f, -0.5f, -0.5f);
        Square square = new Square(A,  B, C, D, Vector3D<float>.Zero);


        Ray ray = new Ray(
            new Vector3D<float>(-5f, 0f, 0), 
            new Vector3D<float>(-1, 0, 0));

        Plane plane = new Plane(A, B , D, Vector3D<float>.Zero);

        Assert.IsFalse(plane.intersect(ray).haveHited);
        Assert.IsFalse(square.intersect(ray));

    }
    
    [Test]
    public void testNotCollingRayInPlaneButNotInSquare()
    {
        Vector3D<float> A = new Vector3D<float>(0.0f, 0.5f, -0.5f);
        Vector3D<float> B = new Vector3D<float>(0.0f, 0.5f, 0.5f);
        Vector3D<float> C = new Vector3D<float>(0.0f, -0.5f, 0.5f);
        Vector3D<float> D = new Vector3D<float>(0.0f, -0.5f, -0.5f);
        Square square = new Square(A,  B, C, D, Vector3D<float>.Zero);


        Ray ray = new Ray(
            new Vector3D<float>(-5f, 2f, 0), 
            new Vector3D<float>(1, 0, 0));

        Plane plane = new Plane(A, B , D, Vector3D<float>.Zero);

        Assert.IsTrue(plane.intersect(ray).haveHited);
        Assert.IsFalse(square.intersect(ray));

    }
    //
    // new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z - 0.5f),
    // new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y + 0.5f, rblock.position.Z - 0.5f),
    // new Vector3D<float>(rblock.position.X - 0.5f, rblock.position.Y - 0.5f, rblock.position.Z + 0.5f),
    //
    //
    [Test]
    public void testLeftFace()
    {
        Vector3D<float> position = Vector3D<float>.Zero;

        Vector3D<float> A = new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z + 0.5f);
        Vector3D<float> B = new Vector3D<float>(position.X - 0.5f, position.Y + 0.5f, position.Z + 0.5f);
        Vector3D<float> C = new Vector3D<float>(position.X - 0.5f, position.Y + 0.5f, position.Z - 0.5f);
        Vector3D<float> D = new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f);
        Square square = new Square(
            D,C,B,A,
            new Vector3D<float>(position.X - 0.5f, position.Y, position.Z)
        );

        
        

        Ray ray = new Ray(
            new Vector3D<float>(-5f, 0f, 0), 
            new Vector3D<float>(1, 0, 0));

        Plane plane = new Plane(D,C,A, new Vector3D<float>(position.X - 0.5f, position.Y, position.Z));

        Assert.IsTrue(plane.intersect(ray).haveHited);
        Assert.IsTrue(square.intersect(ray));

    }
    
    
    [Test]
    public void testLeftFaceOld()
    {
        
        Vector3D<float> position = Vector3D<float>.Zero;

        Ray ray = new Ray(
            new Vector3D<float>(-5f, 0f, 0), 
            new Vector3D<float>(1, 0, 0));

        Plane plane = new Plane(new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X - 0.5f, position.Y + 0.5f, position.Z - 0.5f),
            new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z + 0.5f)
            , new Vector3D<float>(position.X - 0.5f, position.Y, position.Z));

        Assert.IsTrue(plane.intersect(ray).haveHited);
    }
    
    
    
}