using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Silk.NET.Input;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.UI;
using Silk.NET.Maths;


namespace MinecraftCloneSilk.GameComponent
{
    public class Player : GameObject
    {
        private Camera camera;
        private IKeyboard primaryKeyboard;
        private const float moveSpeed = 5.0f;
        private const float sprintSpeed = 10.0f;
        private IMouse mouse;
        private bool debugActivated = false;
        private PlayerUi playerUi;
        private World world;
        private string activeBlockName = "metal";

        private PlayerInteractionToWorld? playerInteractionToWorld;

        public Vector3 position
        {
            get => camera.Position;
            set
            {
                camera.Position = value;
            }
        }
        
        public Player(Game game) : base(game)
        {
            //Start a camera at position 3 on the Z axis, looking at position -1 on the Z axis
            camera = new Camera();
            primaryKeyboard = game.getKeyboard();
            this.mouse = game.getMouse();
            this.playerUi = new PlayerUi(this);
            mouse.MouseDown += onMouseClick;
        }

        

        public Vector3D<float> getDirection3D()
        {
            return new Vector3D<float>(camera.Front.X, camera.Front.Y, camera.Front.Z);
        }

        private void onMouseClick(IMouse mouse, MouseButton mouseButton)
        {
            if(debugActivated) showDebugRayOnClick();
            Block? block = playerInteractionToWorld.getBlock();
            Chunk? chunk = playerInteractionToWorld.getChunk(); 
            if (block.HasValue) {

                if (mouseButton == MouseButton.Left) {
                    Vector3D<int> position = ((Block)block).position + chunk.getPosition();
                    world.setBlock("airBlock", position);
                }

                if (mouseButton == MouseButton.Right && playerInteractionToWorld.getFace().HasValue) {
                    Face face = (Face)playerInteractionToWorld.getFace();
                    world.setBlock( activeBlockName,  chunk.getPosition() +  ((Block)block).position + FaceOffset.getOffsetOfFace(face));
                }
            }
            
        }

       
        public PlayerInteractionToWorld? getPlayerInteractionToWorld()
        {
            return playerInteractionToWorld;
        }

        protected override void update(double deltaTime)
        {
            movePlayer(deltaTime);
            if (mouse.IsButtonPressed(MouseButton.Left)) {
                
            }
        }

        protected override void start()
        {
            this.playerInteractionToWorld = new PlayerInteractionToWorld((World)game.gameObjects[nameof(World)], this);
            world = (World)game.gameObjects[nameof(World)];
        }

        private void movePlayer(double deltaTime)
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


        private void showDebugRayOnClick()
        {
            Console.WriteLine("new debug ray !");
            float raySize = 3;
            new DebugRay(new Vector3D<float>(position.X, position.Y, position.Z) , 
                new Vector3D<float>(position.X + (camera.Front.X * raySize),
                    position.Y + (camera.Front.Y * raySize),
                    position.Z + (camera.Front.Z * raySize)));
        }

        public override void toImGui()
        {
            playerUi.drawUi();
        }
       
        public void debug(bool? setDebug = null)
        {
            debugActivated = setDebug ?? !debugActivated;
        }

    }
}
