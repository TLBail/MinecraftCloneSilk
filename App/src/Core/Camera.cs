using Silk.NET.Input;
using System;
using System.Numerics;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using MinecraftCloneSilk.GameComponent;

namespace MinecraftCloneSilk.Core
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Front { get; set; }

        public Vector3 Up { get; private set; }
        public float AspectRatio { get; set; }

        public float Yaw { get; set; } = -90f;
        public float Pitch { get; set; }

        private float zoom = 45f;
        private Vector2 lastMousePosition;
        private bool isZoomActive = false;
        private IMouse mouse;
        
        public Camera() 
        {
            Setup(Vector3.UnitZ * 6, Vector3.UnitZ * -1, Vector3.UnitY, 800f / 600f);
            Game game = Game.GetInstance();
            IWindow window = game.GetWindow();
            Vector2D<int> size = window.GetFullSize();
            AspectRatio = (float)size.X / (float)size.Y;
            window.FramebufferResize += FrameBufferResize;
            game.mainCamera = this;
            mouse = game.GetMouse();
            mouse.Cursor.CursorMode = CursorMode.Normal;
            mouse.MouseMove += OnMouseMove;
        }

        public void SetZoomActive(bool active) {
            if(isZoomActive == active) return;
            if (active) {
                mouse.Scroll += OnMouseWheel;
            } else {
                mouse.Scroll -= OnMouseWheel;
            }
        }

        private void Setup(Vector3 position, Vector3 front, Vector3 up, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
            Front = front;
            Up = up;
        }

        public void ModifyZoom(float zoomAmount)
        {
            //We don't want to be able to zoom in too close or too far away so clamp to these values
            zoom = Math.Clamp(zoom - zoomAmount, 1.0f, 45f);
        }

        public void ModifyDirection(float xOffset, float yOffset)
        {
            Yaw += xOffset;
            Pitch -= yOffset;

            //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
            Pitch = Math.Clamp(Pitch, -89f, 89f);

            var cameraDirection = Vector3.Zero;
            cameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            cameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
            cameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));

            Front = Vector3.Normalize(cameraDirection);
        }

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(Position, Position + Front, Up);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(zoom), AspectRatio, 0.1f, 1000.0f);
        }

        private unsafe void OnMouseMove(IMouse mouse, Vector2 position)
        {
            var lookSensitivity = 0.1f;
            if (lastMousePosition == default) { lastMousePosition = position; }
            else
            {
                var xOffset = (position.X - lastMousePosition.X) * lookSensitivity;
                var yOffset = (position.Y - lastMousePosition.Y) * lookSensitivity;
                lastMousePosition = position;

                ModifyDirection(xOffset, yOffset);
            }
        }

        private unsafe void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
        {
            ModifyZoom(scrollWheel.Y);
        }

        private void FrameBufferResize(Vector2D<int> size)
        {
            AspectRatio = (float)size.X / (float)size.Y;
        }

    }
}
