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
using Console = MinecraftCloneSilk.UI.Console;


namespace MinecraftCloneSilk.GameComponent
{
    public class Player : GameObject
    {
        private Camera camera;
        private IKeyboard primaryKeyboard;
        private float moveSpeed = 5.0f;
        private const float sprintSpeed = 50.0f;
        private IMouse mouse;
        private bool debugActivated = false;
        private PlayerUi playerUi;
        private World world;
        public string activeBlockName;

        private PlayerInteractionToWorld? playerInteractionToWorld;
        private Console console;
        
        public Vector3 position
        {
            get => camera.Position;
            set
            {
                camera.Position = value;
                console.log(
                    "player position has been set at (" + position.X + "," + position.Y + "," + position.Z + ")");
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
            Block block = playerInteractionToWorld.getBlock();
            Chunk chunk = playerInteractionToWorld.getChunk(); 
            if (block != null) {
                if (mouseButton == MouseButton.Left) {
                    Vector3D<int> position = ((Block)block).position + chunk.getPosition();
                    world.setBlock(BlockFactory.AIR_BLOCK, position);
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
            world = (World)game.gameObjects[typeof(World).FullName];
            this.playerInteractionToWorld = new PlayerInteractionToWorld(world, this);
            playerUi.start(playerInteractionToWorld);
            console = (Console)game.gameObjects[typeof(Console).FullName];
            console.addCommand("/tp", (commandParams) =>
            {
                Vector3 newPosition = Vector3.Zero;
                try {
                    if (commandParams.Length >= 3) {
                        newPosition.X = float.Parse(commandParams[0]);
                        newPosition.Y = float.Parse(commandParams[1]);
                        newPosition.Z = float.Parse(commandParams[2]);
                    }
                }
                catch (Exception e) {
                    newPosition = Vector3.Zero;
                    console.log("Invalid parameters", Console.LogType.ERROR);
                }
                position = newPosition;

            });
        }

        private void movePlayer(double deltaTime)
        {
            var speed = moveSpeed * (float)deltaTime;


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
            float raySize = 20;
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
