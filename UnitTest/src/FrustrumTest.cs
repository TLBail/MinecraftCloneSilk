using System.Numerics;
using MinecraftCloneSilk.Collision;
using MinecraftCloneSilk.Core;

namespace UnitTest;

[TestFixture]
public class FrustrumTest
{

    //by default the camera is at 0,0,0 and looking at 0,0,1
    [Test]
    public void testFrustrum() {
        Camera cam = new Camera();
        Frustrum frustrum = new Frustrum(cam);
        
        AABBCube cube = new AABBCube(new Vector3(0, 0, 10), new Vector3(1, 1, 11));
        Assert.True(cube.IsInFrustrum(frustrum));
        
        AABBCube cube2 = new AABBCube(new Vector3(2, 2, -2), new Vector3(3, 3, -3));
        Assert.False(cube2.IsInFrustrum(frustrum));
    }


    [Test]
    public void testChunkSizeCube() {
        Camera cam = new Camera();
        cam.position = new Vector3(0, 1, 0);
        Frustrum frustrum = new Frustrum(cam);
        
        AABBCube cube2 = new AABBCube(new Vector3(2, 2, 2), new Vector3(18, 18, 18));
        Assert.True(cube2.IsInFrustrum(frustrum));
    }
    
    
    
    [Test]
    public void testChunkSizeCubeCamOffseted() {
        Camera cam = new Camera();
        cam.position = new Vector3(200, 0, 0);
        Frustrum frustrum = new Frustrum(cam);
        
        AABBCube cube = new AABBCube(new Vector3(202, 2, 2), new Vector3(218, 18, 18));
        Assert.True(cube.IsInFrustrum(frustrum));
        
        AABBCube cube2 = new AABBCube(new Vector3(2, 2, 2), new Vector3(18, 18, 18));
        Assert.False(cube2.IsInFrustrum(frustrum));
    }
    
}