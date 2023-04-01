using System.Numerics;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.Core;

public struct LineVertex
{
    private Vector3D<float> position;
    private Vector3D<float> color;

    public LineVertex(Vector3D<float> position, Vector3D<float> color)
    {
        this.position = position;
        this.color = color;
    }
}
public enum LineType
{
    STRIP,
    LOOP,
    LINE
}

public class Line
{
   
    
    private static readonly Vector3D<float> DEFAULT_COLOR = new Vector3D<float>(1.0f, 0, 0);

    private BufferObject<LineVertex> Vbo;
    private VertexArrayObject<LineVertex, uint> Vao;
    
    private static Shader rayShader;
    private GL Gl;
    private Game game;
    
    private int nbVertices;
    private LineType lineType;
    
    public Line(Vector3D<float> start, Vector3D<float> end, Vector3D<float> color, LineType lineType = LineType.LINE)
    : this(start, end, color, color, lineType) {    }

    public Line(Vector3D<float> start, Vector3D<float> end, Vector3D<float> startColor, Vector3D<float> endColor, LineType lineType = LineType.LINE) : this(new[]
        {
            new LineVertex(start, startColor),
            new LineVertex(end, endColor),
        }, lineType) { }

    public Line(LineVertex[] vertices, LineType lineType = LineType.LINE) {
        game = Game.getInstance();
        Gl = game.getGL();
        game.drawables += Drawables;
        nbVertices = vertices.Length;
        this.lineType = lineType;
        
        Vbo = new BufferObject<LineVertex>(Gl, vertices, BufferTargetARB.ArrayBuffer);
        Vao = new VertexArrayObject<LineVertex, uint>(Gl, Vbo);
        
        Vao.Bind();
        Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
        Vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, "color");

        if (rayShader == null) {
            rayShader = new Shader(Gl, "./Shader/3dPosOneColorUni/VertexShader.glsl",
                "./Shader/3dPosOneColorUni/FragmentShader.glsl");
        }   
    }


    public Line(Vector3D<float> start, Vector3D<float> end) : this(start,  end, DEFAULT_COLOR){}

    public void remove()
    {
        game.drawables -= Drawables;
        Vao.Dispose();
        Vbo.Dispose();
    }
    
    private void Drawables(GL gl, double deltatime)
    {
        Vao.Bind();
        rayShader.Use();
        
        
        var model = Matrix4x4.Identity;
        model = Matrix4x4.CreateTranslation(new Vector3(0,0,0));
        rayShader.SetUniform("model", model);

        switch (lineType) {
            case LineType.LINE:
                Gl.DrawArrays(PrimitiveType.Lines, 0, (uint)nbVertices);
                break;
            case LineType.LOOP:
                Gl.DrawArrays(PrimitiveType.LineLoop, 0, (uint)nbVertices);
                break;
            case LineType.STRIP:
                Gl.DrawArrays(PrimitiveType.LineStrip, 0, (uint)nbVertices);
                break;
        }
    }
}