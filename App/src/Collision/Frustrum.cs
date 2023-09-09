using System.Numerics;
using MinecraftCloneSilk.Core;

namespace MinecraftCloneSilk.Collision;

public class Frustrum
{
    public Plane topFace{get; private set;}
    public Plane bottomFace{get; private set;}
    
    public Plane rightFace{get; private set;}
    public Plane leftFace{get; private set;}
    
    public Plane farFace{get; private set;}
    public Plane nearFace{get; private set;}

    public Frustrum(Camera cam) {
        Update(cam);
    }

    public void Update(Camera cam) {
        float fovY = MathHelper.DegreesToRadians(cam.zoom);
        float halfVSide = cam.farPlane * MathF.Tan(fovY * .5f);
        float halfHSide = halfVSide * cam.aspectRatio;
        Vector3 frontMultFar = cam.farPlane * cam.Front;

        nearFace = new Plane( cam.Front,   cam.position + cam.nearPlane * cam.Front);
        farFace = new Plane( -cam.Front ,   cam.position + frontMultFar);
        rightFace = new Plane(Vector3.Cross(frontMultFar - cam.Right * halfHSide, cam.up), cam.position );
        leftFace = new Plane(Vector3.Cross(cam.up,frontMultFar + cam.Right * halfHSide), cam.position );
        topFace = new Plane(Vector3.Cross(cam.Right, frontMultFar - cam.up * halfVSide), cam.position );
        bottomFace = new Plane(Vector3.Cross(frontMultFar + cam.up * halfVSide, cam.Right), cam.position );

    }


}