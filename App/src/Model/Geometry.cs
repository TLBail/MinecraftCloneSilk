using System.Numerics;

namespace MinecraftCloneSilk.Model;

public class Geometry
{
    public static readonly Vector3[] TopFaceVerticesOffset =
    [
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
    ];
        
    public static readonly Vector3[] BottomFaceVerticesOffset =
    [
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
    ];

    public static readonly Vector3[] LeftFaceVerticesOffset =
    [
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
    ];

    public static readonly Vector3[] RightFaceVerticesOffset =
    [
        new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
    ];
        
    public static readonly Vector3[] FrontFaceVerticesOffset =
    [
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
    ];

    public static readonly Vector3[] BackFaceVerticesOffset =
    [
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f),
    ];

    public static Vector3[] verticesOffsets;

    static Geometry(){
        verticesOffsets = new Vector3[36];
        TopFaceVerticesOffset.CopyTo(verticesOffsets, 0);
        BottomFaceVerticesOffset.CopyTo(verticesOffsets, 6);
        LeftFaceVerticesOffset.CopyTo(verticesOffsets, 12);
        RightFaceVerticesOffset.CopyTo(verticesOffsets, 18);
        FrontFaceVerticesOffset.CopyTo(verticesOffsets, 24);
        BackFaceVerticesOffset.CopyTo(verticesOffsets, 30);
    }

        
        
}