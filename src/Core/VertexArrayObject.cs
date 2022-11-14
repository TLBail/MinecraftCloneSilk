using Silk.NET.OpenGL;
using System;
using System.Runtime.InteropServices;
using MinecraftCloneSilk.GameComponent;


namespace MinecraftCloneSilk.Core
{
    public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        private uint _handle;
        private GL _gl;
        private bool disposed = false;
        
        public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo) : this(gl, vbo, null){ }

        public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
        {
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

        ~VertexArrayObject() {
            Dispose(false);
        }

        protected void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    _gl.DeleteVertexArray(_handle);
                }
                disposed = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
