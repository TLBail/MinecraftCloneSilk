using Silk.NET.OpenGL;
using System;
using System.Runtime.InteropServices;


namespace MinecraftCloneSilk.src
{
    public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        private uint _handle;
        private GL _gl;

        public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo) : this(gl, vbo, null){ }

        public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
        {
            Game.getInstance().disposables += Dispose;
            _gl = gl;

            _handle = _gl.GenVertexArray();
            Bind();
            vbo.Bind();
            ebo?.Bind();
        }

        public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, string fieldName)
        {
            _gl.VertexAttribPointer(index, count, type, false,(uint) sizeof(TVertexType), (void*) (Marshal.OffsetOf(typeof(TVertexType), fieldName)));
            _gl.EnableVertexAttribArray(index);
        }

        public void Bind()
        {
            _gl.BindVertexArray(_handle);
        }

        public void Dispose()
        {
            _gl.DeleteVertexArray(_handle);
        }
    }
}
