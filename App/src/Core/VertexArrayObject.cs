using Silk.NET.OpenGL;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MinecraftCloneSilk.GameComponent;


namespace MinecraftCloneSilk.Core
{
    public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        private uint handle;
        private GL gl;
        private bool disposed = false;

        public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType>? ebo = null)
        {
            this.gl = gl;

            handle = this.gl.GenVertexArray();
            Bind();
            vbo.Bind(BufferTargetARB.ArrayBuffer);
            ebo?.Bind(BufferTargetARB.ElementArrayBuffer);
        }

        /**
         * <summary>Bind a vertex attribute to a field of the vertex type
         *  the type of the field is converted to a float vector
         * </summary>
         * <param name="index">The index of the vertex attribute</param>
         * <param name="count">The number of components of the vertex attribute</param>
         * <param name="type">The type of the vertex attribute</param>
         * <param name="fieldName">The name of the field of the vertex type</param>
         */
        public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, string fieldName)
            => VertexAttributePointer(index, count, type,(int)Marshal.OffsetOf(typeof(TVertexType), fieldName));
        public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, int offset)
        {
            gl.VertexAttribPointer(index, count, type, false,(uint) sizeof(TVertexType), (void*)offset);
            gl.EnableVertexAttribArray(index);
        }
        
        
        /**
         * <summary>Bind a vertex attribute to a field of the vertex type
         * only the integer types are accepted
         * </summary>
         * <param name="index">The index of the vertex attribute</param>
         * <param name="count">The number of components of the vertex attribute</param>
         * <param name="type">The type of the vertex attribute</param>
         * <param name="fieldName">The name of the field of the vertex type</param>
         */
        public unsafe void VertexAttributeIPointer(uint index, int count,  VertexAttribIType type, string fieldName) => 
            VertexAttributeIPointer(index, count, type,(int)Marshal.OffsetOf(typeof(TVertexType), fieldName));
        public unsafe void VertexAttributeIPointer(uint index, int count,  VertexAttribIType type, int offset)
        {
            gl.VertexAttribIPointer(index, count, type,(uint) sizeof(TVertexType), (void*) (offset));
            gl.EnableVertexAttribArray(index);
        }

        public void Bind()
        {
            gl.BindVertexArray(handle);
        }

        ~VertexArrayObject() {
            Dispose(false);
        }

        protected void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    gl.DeleteVertexArray(handle);
                }
                disposed = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffset<TF>(ref TVertexType vertex, ref TF field) {
            return Unsafe.ByteOffset(ref Unsafe.As<TVertexType, byte>(ref vertex), ref Unsafe.As<TF, byte>(ref field)).ToInt32();
        }
    }
}
