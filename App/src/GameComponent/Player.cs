using System.Numerics;
using Silk.NET.Input;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
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
        private const float SPRINT_SPEED = 50.0f;
        private IMouse? mouse;
        private bool debugActivated = false;
        private PlayerUi playerUi;
        private World world = null!;

        private PlayerInteractionToWorld playerInteractionToWorld = null!;
        private Console console = null!;
        
        
        public Inventaire inventaire;
        
        public Vector3 position
        {
            get => camera.position;
            set
            {
                camera.position = value;
                console.Log(
                    "player position has been set at (" + position.X + "," + position.Y + "," + position.Z + ")");
            }
        }
        
        public Player(Game game) : base(game)
        {
            //Start a camera at position 3 on the Z axis, looking at position -1 on the Z axis
            camera = new Camera(game.GetWindow(), game.GetMouse());
            game.mainCamera = camera;
            primaryKeyboard = game.GetKeyboard();
            this.mouse = game.GetMouse();
            this.playerUi = new PlayerUi(this);
            mouse.MouseDown += OnMouseClick;
            inventaire = new Inventaire(this);
        }

        public Vector3 GetDirection3D() {
            return camera.Front;
        }
        
        public void Click(MouseButton mouseButton)
        {
            OnMouseClick(mouse!, mouseButton);
        }

        private void OnMouseClick(IMouse mouse, MouseButton mouseButton)
        {
            if(debugActivated) ShowDebugRayOnClick();
            Block? block = playerInteractionToWorld.GetBlock();
            Chunk? chunk = playerInteractionToWorld.GetChunk(); 
            if (block != null && chunk != null) {
                if (mouseButton == MouseButton.Left) {
                    Vector3D<int> position = ((Block)block).position + chunk.position;
                    world.SetBlock(BlockFactory.AIR_BLOCK, position);
                }
                if (mouseButton == MouseButton.Right){
                    Face? face = playerInteractionToWorld.GetFace();
                    if (face is not null && inventaire.HaveBlockToPlace()) {
                        world.SetBlock(inventaire.GetActiveBlock()!.block.name,
                            chunk.position + block.position + FaceOffset.GetOffsetOfFace(face.Value));
                    }
                }
            }
        }

       
        public PlayerInteractionToWorld GetPlayerInteractionToWorld()
        {
            return playerInteractionToWorld;
        }

        protected override void Update(double deltaTime)
        {
            MovePlayer(deltaTime);
        }

        protected override void Start()
        {
            world = (World)game.gameObjects[typeof(World).FullName!];
            this.playerInteractionToWorld = new PlayerInteractionToWorld(world, this);
            playerUi.Start(playerInteractionToWorld);
            console = (Console)game.gameObjects[typeof(Console).FullName!];
            console.AddCommand("/tp", (commandParams) =>
            {
                Vector3 newPosition = Vector3.Zero;
                try {
                    if (commandParams.Length >= 3) {
                        newPosition.X = float.Parse(commandParams[0]);
                        newPosition.Y = float.Parse(commandParams[1]);
                        newPosition.Z = float.Parse(commandParams[2]);
                    }
                }
                catch (Exception) {
                    newPosition = Vector3.Zero;
                    console.Log("Invalid parameters", Console.LogType.ERROR);
                }
                position = newPosition;

            });
        }

        private void MovePlayer(double deltaTime)
        {
            var speed = moveSpeed * (float)deltaTime;


            if (primaryKeyboard.IsKeyPressed(Key.ShiftLeft)) speed = SPRINT_SPEED * (float)deltaTime;

                
            if (primaryKeyboard.IsKeyPressed(Key.W))
            {
                //Move forwards
                camera.position += speed * camera.Front;
            }
            if (primaryKeyboard.IsKeyPressed(Key.S))
            {
                //Move backwards
                camera.position -= speed * camera.Front;
            }
            if (primaryKeyboard.IsKeyPressed(Key.A))
            {
                //Move left
                camera.position -= Vector3.Normalize(Vector3.Cross(camera.Front, camera.up)) * speed;
            }
            if (primaryKeyboard.IsKeyPressed(Key.D))
            {
                //Move right
                camera.position += Vector3.Normalize(Vector3.Cross(camera.Front, camera.up)) * speed;
            }

            if (primaryKeyboard.IsKeyPressed(Key.Space))
            {
                //move up
                camera.position += camera.up * speed;
            }
            if (primaryKeyboard.IsKeyPressed(Key.Q))
            {
                //move up
                camera.position += -camera.up * speed;
            }
        }


        private void ShowDebugRayOnClick()
        {
            float raySize = 20;
            new Line(new Vector3D<float>(position.X, position.Y, position.Z) , 
                new Vector3D<float>(position.X + (camera.Front.X * raySize),
                    position.Y + (camera.Front.Y * raySize),
                    position.Z + (camera.Front.Z * raySize)));
        }

        public override void ToImGui()
        {
            playerUi.DrawUi();
        }
        
        
       
        public void Debug(bool? setDebug = null)
        {
            debugActivated = setDebug ?? !debugActivated;
        }

    }
}
