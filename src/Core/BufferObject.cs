using Silk.NET.OpenGL;
using System;
using Microsoft.VisualBasic.CompilerServices;
using MinecraftCloneSilk.GameComponent;

namespace MinecraftCloneSilk.Core
{
    public class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        private uint _handle;
        private BufferTargetARB _bufferType;
        private GL _gl;

        public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
        {
            Game.getInstance().disposables += Dispose;
            _gl = gl;
            _bufferType = bufferType;

    
            _handle = _gl.GenBuffer();
            Bind();
            fixed (void* d = data)
            {
                _gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
            }
        }

        public unsafe BufferObject(GL gl, int nbVertex, BufferTargetARB bufferType)
        {
            Game.getInstance().disposables += Dispose;
            this._gl = gl;
            _bufferType = bufferType;

            _handle = gl.GenBuffer();
            Bind();
            gl.BufferData(bufferType, (nuint)(nbVertex * sizeof(TDataType)), null, BufferUsageARB.DynamicDraw);
        }

        public void  sendData(ReadOnlySpan<TDataType> data, nint offset)
        {
            _gl.BufferSubData(_bufferType, offset,  data);
        }
        
        
        public void Bind()
        {
            _gl.BindBuffer(_bufferType, _handle);
        }

        public void Dispose()
        {
            _gl.DeleteBuffer(_handle);
        }
    }

}
