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

public class Line : IDisposable
{
   
    
    private static readonly Vector3D<float> DefaultColor = new Vector3D<float>(1.0f, 0, 0);

    private BufferObject<LineVertex> vbo;
    private VertexArrayObject<LineVertex, uint> vao;
    
    private static Shader? rayShader;
    private GL gl;
    private Game game;
    
    private int nbVertices;
    private LineType lineType;
    
    public Line(Vector3D<float> start, Vector3D<float> end, Vector3D<float> color, LineType lineType = LineType.LINE)
    : this(start, end, color, color, lineType) {    }

    public Line(Vector3D<float> start, Vector3D<float> end, Vector3D<float> startColor, Vector3D<float> endColor, LineType lineType = LineType.LINE) : this([
            new LineVertex(start, startColor),
            new LineVertex(end, endColor),
        ], lineType) { }

    public Line(LineVertex[] vertices, LineType lineType = LineType.LINE) {
        game = Game.GetInstance();
        gl = game.GetGl();
        game.drawables += Drawables;
        nbVertices = vertices.Length;
        this.lineType = lineType;
        
        vbo = new BufferObject<LineVertex>(gl, vertices, BufferTargetARB.ArrayBuffer);
        vao = new VertexArrayObject<LineVertex, uint>(gl, vbo);
        
        vao.Bind();
        vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
        vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, "color");

        if (rayShader == null) {
            rayShader = new Shader(gl, Generated.FilePathConstants.__Shader_3dPosOneColorUni.VertexShader_glsl,
                Generated.FilePathConstants.__Shader_3dPosOneColorUni.FragmentShader_glsl);
        }   
    }


    public Line(Vector3D<float> start, Vector3D<float> end) : this(start,  end, DefaultColor){}
    public Line(Vector3 start, Vector3 end) : this(new Vector3D<float>(start.X, start.Y, start.Z), new Vector3D<float>(end.X, end.Y, end.Z)){}

    public void Dispose()
    {
        game.drawables -= Drawables;
        vao.Dispose();
        vbo.Dispose();
    }
    
    private void Drawables(GL gl, double deltatime)
    {
        vao.Bind();
        rayShader!.Use();
        
        
        Matrix4x4 model = Matrix4x4.CreateTranslation(new Vector3(0,0,0));
        rayShader.SetUniform("model", model);

        switch (lineType) {
            case LineType.LINE:
                this.gl.DrawArrays(PrimitiveType.Lines, 0, (uint)nbVertices);
                break;
            case LineType.LOOP:
                this.gl.DrawArrays(PrimitiveType.LineLoop, 0, (uint)nbVertices);
                break;
            case LineType.STRIP:
                this.gl.DrawArrays(PrimitiveType.LineStrip, 0, (uint)nbVertices);
                break;
        }
    }
    
}