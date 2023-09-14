using System.Drawing;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Voxel.Common;

namespace Voxel.Client;

public class DrawHelper {
    public IWindow Win {get; private set;}
    public GL? Gl {get; private set;}
    private bool IsInit = false;

    private readonly float[] vertices = {
        0.5f,  0.5f, 1.0f,
        0.5f, -0.5f, 1.0f,
        -0.5f, -0.5f, 0.0f,
        -0.5f,  0.5f, 0.0f
    };
    private readonly uint[] indices = {
        0u, 1u, 3u,
        1u, 2u, 3u
    };

    private uint vao;
    private uint vbo;
    private uint ebo;
    private uint program;

    public readonly string VtxShader = ResourceHelper.GetResourceAsString("Main.vert")!;
    public readonly string FragShader = ResourceHelper.GetResourceAsString("Main.frag")!;

    public DrawHelper(IWindow win) {
        Win = win;
    }

    public void Init() {
        if (IsInit)
            return;
        
        IsInit = true;

        Gl = Win.CreateOpenGL();

        Gl.ClearColor(Color.White);

        vao = Gl.GenVertexArray();
        vbo = Gl.GenBuffer();
        ebo = Gl.GenBuffer();

        Gl.BindVertexArray(vao);
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        unsafe {
            fixed (float* buf = vertices)
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

            fixed (uint* buf = indices)
                Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
        }

        uint vtx = Gl.CreateShader(ShaderType.VertexShader);
        Gl.ShaderSource(vtx, VtxShader);
        Gl.CompileShader(vtx);

        Gl.GetShader(vtx, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vtx));
        
        uint frag = Gl.CreateShader(ShaderType.FragmentShader);
        Gl.ShaderSource(frag, FragShader);
        Gl.CompileShader(frag);

        Gl.GetShader(frag, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(frag));
        
        program = Gl.CreateProgram();
        Gl.AttachShader(program, vtx);
        Gl.AttachShader(program, frag);
        Gl.LinkProgram(program);
        Gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + Gl.GetProgramInfoLog(program));
        
        Gl.DetachShader(program, vtx);
        Gl.DetachShader(program, frag);
        Gl.DeleteShader(vtx);
        Gl.DeleteShader(frag);

        const uint positionLoc = 0;
        Gl.EnableVertexAttribArray(positionLoc);
        unsafe {
            Gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*) 0);
        }
        Gl.BindVertexArray(0);
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

    }

    public void Render(double delta) {
        Gl!.Clear(ClearBufferMask.ColorBufferBit);
        Gl.BindVertexArray(vao);
        Gl.UseProgram(program);
        unsafe {
            Gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*) 0);
        }
    }
}