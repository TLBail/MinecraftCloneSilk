using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Silk.NET.Input;
using MinecraftCloneSilk.Core;
using Silk.NET.Maths;


namespace MinecraftCloneSilk.GameComponent
{
    public class Player
    {
        private Camera camera;
        private IKeyboard primaryKeyboard;
        private const float moveSpeed = 5.0f;
        private const float sprintSpeed = 10.0f;
        private IMouse mouse;
        private bool debugActivated = false;
        
        public Vector3 position
        {
            get => camera.Position;
            set
            {
                camera.Position = value;
            }
        }
        
        public Player()
        {
            //Start a camera at position 3 on the Z axis, looking at position -1 on the Z axis
            camera = new Camera();
            Game game = Game.getInstance();
            primaryKeyboard = game.getKeyboard();
            game.updatables += Update;
            this.mouse = game.getMouse();
        }

        private void onMouseClick(IMouse arg1, MouseButton arg2)
        {
            Console.WriteLine("new debug ray !");
            float raySize = 3;
            new DebugRay(new Vector3D<float>(position.X, position.Y, position.Z) , 
                new Vector3D<float>(position.X + (camera.Front.X * raySize),
                    position.Y + (camera.Front.Y * raySize),
                    position.Z + (camera.Front.Z * raySize)));
        }

        public void debug(bool? setDebug = null)
        {
            debugActivated = setDebug ?? !debugActivated;

            if (debugActivated) {
                mouse.MouseDown += onMouseClick;
            }
            else {
                mouse.MouseDown -= onMouseClick;
            }
        }

        public void Update(double deltaTime)
        {
            var speed = Player.moveSpeed * (float)deltaTime;


            if (primaryKeyboard.IsKeyPressed(Key.ShiftLeft)) speed = sprintSpeed * (float)deltaTime;

                
            if (primaryKeyboard.IsKeyPressed(Key.W))
            {
                //Move forwards
                camera.Position += speed * camera.Front;
            }
            if (primaryKeyboard.IsKeyPressed(Key.S))
            {
                //Move backwards
                camera.Position -= speed * camera.Front;
            }
            if (primaryKeyboard.IsKeyPressed(Key.A))
            {
                //Move left
                camera.Position -= Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * speed;
            }
            if (primaryKeyboard.IsKeyPressed(Key.D))
            {
                //Move right
                camera.Position += Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * speed;
            }

            if (primaryKeyboard.IsKeyPressed(Key.Space))
            {
                //move up
                camera.Position += camera.Up * speed;
            }
            if (primaryKeyboard.IsKeyPressed(Key.Q))
            {
                //move up
                camera.Position += -camera.Up * speed;
            }

        }
    }
}
