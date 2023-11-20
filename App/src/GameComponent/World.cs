using System.Numerics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using MinecraftCloneSilk.Model.WorldGen;
using MinecraftCloneSilk.UI;
using Silk.NET.Maths;
using Console = MinecraftCloneSilk.UI.Console;

namespace MinecraftCloneSilk.GameComponent;

public enum WorldMode
{
    EMPTY, // no chunks
    SIMPLE, //juste couple of chunk around 0
    DYNAMIC // chunks generated around player
}

public class World : GameObject
{
    private Player player = null!;
    public int radius { get; set; } = 6;
    private readonly WorldUi worldUi;
    public IWorldGenerator worldGeneration;
    public WorldMode worldMode { get; set; }
    public ChunkManager chunkManager;

    private Vector3D<int> lastPlayerChunkPosition = new Vector3D<int>(-1);

    private RegionStorage regionStorage;
    
    public Lighting lighting { get; private set; }

    public World(Game game, WorldMode worldMode = WorldMode.EMPTY) : base(game) {
        this.worldMode = worldMode;
        this.lighting = new Lighting();
        worldUi = new WorldUi(this, lighting);
        worldGeneration = new WorldNaturalGeneration();
        regionStorage = new RegionStorage("./Worlds/newWorld");
        chunkManager = new ChunkManager(radius, worldGeneration, regionStorage);
    }


    protected override void Start() {
        player = (Player)game.gameObjects[typeof(Player).FullName!];
        if (worldMode == WorldMode.SIMPLE) {
            AddExempleChunk();
        }

        AddCommand();
    }

    [Logger.Timer]
    protected override void Update(double deltaTime) {
        if (worldMode == WorldMode.DYNAMIC) {
            CreateChunkAroundPlayer();
        }
        chunkManager.Update(deltaTime);
    }

    protected override void Stop() {
        regionStorage.Dispose();
    }


    public void SetBlock(string blockName, Vector3D<int> position) {
        if (blockName == null) {
            throw new GameException(this, "try to set a block with a name null");
        }
        var key = GetChunkPosition(position);
        var chunk = chunkManager.GetBlockGeneratedChunk(key);
        var localPosition = GetLocalPosition(position);
        chunk.SetBlock(localPosition.X, localPosition.Y, localPosition.Z, blockName);
    }

    public Block GetBlock(Vector3D<int> position) {
        var key = GetChunkPosition(position);
        var localPosition = GetLocalPosition(position);
        var chunk = chunkManager.GetBlockGeneratedChunk(key);
        return chunk.GetBlock(localPosition);
    }


    public void SetWorldMode(WorldMode worldMode) {
        this.worldMode = worldMode;
        switch (worldMode) {
            case WorldMode.EMPTY:
                if (chunkManager.Count() > 0) {
                    chunkManager.Clear();
                }

                break;
            case WorldMode.SIMPLE:
                if (chunkManager.Count() == 0) {
                    AddExempleChunk();
                }

                break;
            case WorldMode.DYNAMIC:
                CreateChunkAroundPlayer();
                break;
        }
    }

    public List<Chunk> GetWorldChunks() {
        return chunkManager.GetChunksList();
    }

    public bool ContainChunkKey(Vector3D<int> key) {
        return chunkManager.ContainsKey(key);
    }

    public Chunk GetChunk(Vector3D<int> position) {
        position = GetChunkPosition(position);
        return chunkManager.GetChunk(position);
    }


    public static Vector3D<int> GetChunkPosition(Vector3D<int> blockPosition) {
        return new Vector3D<int>(
            (int)((int)MathF.Floor((float)blockPosition.X / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE),
            (int)((int)MathF.Floor((float)blockPosition.Y / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE),
            (int)((int)MathF.Floor((float)blockPosition.Z / Chunk.CHUNK_SIZE) * Chunk.CHUNK_SIZE)
        );
    }

    public static Vector3D<int> GetLocalPosition(Vector3D<int> globalPosition) {
        var localPosition = new Vector3D<int>((int)(globalPosition.X % Chunk.CHUNK_SIZE),
            (int)(globalPosition.Y % Chunk.CHUNK_SIZE), (int)(globalPosition.Z % Chunk.CHUNK_SIZE));
        if (localPosition.X < 0) {
            localPosition.X = (int)(Chunk.CHUNK_SIZE + localPosition.X);
        }

        if (localPosition.Y < 0) {
            localPosition.Y = (int)(Chunk.CHUNK_SIZE + localPosition.Y);
        }

        if (localPosition.Z < 0) {
            localPosition.Z = (int)(Chunk.CHUNK_SIZE + localPosition.Z);
        }

        return localPosition;
    }

    private void AddCommand() {
        Console console = (Console)game.gameObjects[typeof(Console).FullName!];
        console.AddCommand("/addChunk", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> newPosition = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                chunkManager.AddChunkToLoad(newPosition);
                console.Log("chunk at " + newPosition + " added succefuly"
                );
            }
        });

        console.AddCommand("/rmChunk", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                console.Log("chunk at " + position + " unloaded " + (
                        chunkManager.TryToUnloadChunk(position) ? "succefuly" : "failed")
                );
            }
        });

        console.AddCommand("/setBlock", (commandParams) =>
        {
            if (commandParams.Length >= 4) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                SetBlock(commandParams[3], position);
                console.Log("block " + commandParams[3] + " at " + position + " added succefuly"
                );
            }
        });

        console.AddCommand("/getBlock", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                console.Log("block at " + position + " is " + GetBlock(position).name
                );
            }
        });

        console.AddCommand("/getChunk", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                console.Log("chunk at " + position + " is \n" + GetChunk(position).ToString()
                );
            }
        });
        console.AddCommand("/blockLine", (commandParams) =>
        {
            if (commandParams.Length == 2) {
                int nbBlock = int.Parse(commandParams[0]);
                string blockName = commandParams[1];
                
                Vector3 position = new Vector3(
                    (int)player.position.X,
                    (int)player.position.Y,
                    (int)player.position.Z
                );
                
                
                for (int i = 0; i < nbBlock; i++) {
                    SetBlock(blockName, new Vector3D<int>((int)position.X, (int)position.Y, (int)position.Z));
                    position += player.GetDirection3D();
                }
                
            } else {
                console.Log("require the number of block and the block name");
            }
        });
        console.AddCommand("/bomb", (commandParams) =>
        {
            int size = 10;
            if (commandParams.Length >= 1) size = int.Parse(commandParams[0]);
            
            Vector3D<int> position = new Vector3D<int>(
                (int)player.position.X,
                (int)player.position.Y,
                (int)player.position.Z
            );
            
            for (int x = -size; x < size; x++) {
                for (int y = -size; y < size; y++) {
                    for (int z = -size; z < size; z++) {
                        Vector3D<int> positionS = position + new Vector3D<int>(x, y, z);
                        if(Vector3D.Distance(positionS, position) < size)
                            SetBlock("air", positionS);
                    }
                }
            }
        });
        
        console.AddCommand("/wall", (commandParams) =>
        {
            if (commandParams.Length == 2) {
                int size = int.Parse(commandParams[0]);
                string blockName = commandParams[1];
                Vector3 position = player.position + player.GetDirection3D() * size;
                Vector3D<int> centerWall = new Vector3D<int>(
                    (int)position.X,
                    (int)position.Y,
                    (int)position.Z);
                for (int x = 0; x < size; x++) {
                    for (int y = 0; y < size; y++) {
                        SetBlock(blockName, centerWall + (new Vector3D<int>(x, y, 0) - new Vector3D<int>(size / 2, size / 2, 0)));
                    }
                }

            }else {
                console.Log("require the size and the block name");
            }

            
        });
        
        console.AddCommand("/spiral", (commandParams) =>
        {
            if (commandParams.Length == 3)
            {
                int nbTurns = int.Parse(commandParams[0]); // Nombre de tours de la spirale
                string blockName = commandParams[1]; // Nom du bloc à utiliser pour la spirale
                float heightIncrement = float.Parse(commandParams[2]); // Incrément de hauteur après chaque tour complet

                Vector3 position = new Vector3(
                    (int)player.position.X,
                    (int)player.position.Y,
                    (int)player.position.Z
                );

                float angle = 0;
                float radius = 0;
                float height = 0;

                // Détermine le nombre de blocs en fonction du nombre de tours et de la précision
                int totalBlocks = (int)(nbTurns * 360 * 1.5f);

                for (int i = 0; i < totalBlocks; i++)
                {
                    // Convertit l'angle et le rayon en coordonnées X et Z
                    float x = radius * MathF.Cos(MathHelper.DegreesToRadians(angle));
                    float z = radius * MathF.Sin(MathHelper.DegreesToRadians(angle));

                    // Place le bloc
                    SetBlock(blockName, new Vector3D<int>((int)(position.X + x), (int)(position.Y + height), (int)(position.Z + z)));

                    // Augmente l'angle, le rayon et la hauteur
                    angle += 0.4f; // Plus cette valeur est petite, plus la spirale sera serrée
                    radius += 0.01f; // Augmente le rayon progressivement
                    height += heightIncrement / 360; // Augmente la hauteur après chaque tour complet

                    if (angle >= 360 * nbTurns)
                    {
                        break; // Sort de la boucle une fois le nombre de tours atteint
                    }
                }
            }
            else
            {
                console.Log("require the number of turns, block name, and height increment");
            }
        });

        
        console.AddCommand("/sierpinski", (commandParams) =>
        {
            if (commandParams.Length == 3)
            {
                int level = int.Parse(commandParams[0]); // Niveau de récursivité du fractal
                string blockName = commandParams[1]; // Nom du bloc à utiliser
                int size = int.Parse(commandParams[2]); // Taille de base du triangle

                Vector3 startPosition = new Vector3(
                    (int)player.position.X,
                    (int)player.position.Y,
                    (int)player.position.Z
                );

                DrawSierpinski(startPosition, level, size, blockName);
            }
            else
            {
                console.Log("require the recursion level, block name, and size");
            }
        });
        
        console.AddCommand("/mengerSponge", (commandParams) =>
        {
            if (commandParams.Length == 3)
            {
                int level = int.Parse(commandParams[0]); // Niveau de récursivité du fractal
                string blockName = commandParams[1]; // Nom du bloc à utiliser
                int size = int.Parse(commandParams[2]); // Taille initiale du cube

                Vector3 startPosition = new Vector3(
                    (int)player.position.X,
                    (int)player.position.Y,
                    (int)player.position.Z
                );

                DrawMengerSponge(startPosition, level, size, blockName);
            }
            else
            {
                console.Log("require the recursion level, block name, and size");
            }
        });
        
      
    }

    private void CreateChunkAroundPlayer() {
        var centerChunk =
            GetChunkPosition(new Vector3D<int>((int)player.position.X, (int)player.position.Y, (int)player.position.Z));
        if (lastPlayerChunkPosition == centerChunk) return;
        lastPlayerChunkPosition = centerChunk;
        var chunkRelevant = new List<Vector3D<int>>();
        var rootChunk = centerChunk + new Vector3D<int>((int)(-radius * Chunk.CHUNK_SIZE));
        for (var x = 0; x < 2 * radius; x++)
        for (var y = 0; y < 2 * radius; y++)
        for (var z = 0; z < 2 * radius; z++) {
            var key = rootChunk + new Vector3D<int>((int)(x * Chunk.CHUNK_SIZE), (int)(y * Chunk.CHUNK_SIZE),
                (int)(z * Chunk.CHUNK_SIZE));
            chunkRelevant.Add(key);
        }

        chunkManager.UpdateRelevantChunks(chunkRelevant);
    }


    private void AddExempleChunk() {
        Vector3D<int>[] postions =
        {
            Vector3D<int>.Zero,
            new Vector3D<int>(Chunk.CHUNK_SIZE, 0, 0),
            
            //new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, (int)Chunk.CHUNK_SIZE),
            //new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, -(int)Chunk.CHUNK_SIZE),
            //new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, 0),
            //new Vector3D<int>(0, 0, -(int)Chunk.CHUNK_SIZE),
            //new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, (int)Chunk.CHUNK_SIZE),
            //new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, -(int)Chunk.CHUNK_SIZE),
            //new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, 0),
            //new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, (int)Chunk.CHUNK_SIZE),
            //new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, -(int)Chunk.CHUNK_SIZE)
        };
        chunkManager.AddChunksToLoad(postions.ToList());
    }

    public override void ToImGui() {
        worldUi.DrawUi();
    }
    
    void DrawSierpinski(Vector3 position, int level, int size, string blockName)
    {
        if (level == 0)
        {
            // Dessine un triangle simple à la position donnée
            DrawTriangle(position, size, blockName);
        }
        else
        {
            int newSize = size / 2;
            // Dessine 3 triangles de Sierpinski de niveau inférieur
            DrawSierpinski(position, level - 1, newSize, blockName);
            DrawSierpinski(new Vector3(position.X + newSize, position.Y, position.Z), level - 1, newSize, blockName);
            DrawSierpinski(new Vector3(position.X + newSize / 2, position.Y, position.Z + (int)(newSize * Math.Sqrt(3) / 2)), level - 1, newSize, blockName);
        }
    }

    void DrawTriangle(Vector3 position, int size, string blockName)
    {
        for (int y = 0; y <= size; y++)
        {
            for (int x = 0; x <= y; x++)
            {
                SetBlock(blockName, new Vector3D<int>((int)position.X + x, (int)position.Y, (int)position.Z + y));
                SetBlock(blockName, new Vector3D<int>((int)position.X - x, (int)position.Y, (int)position.Z + y));
            }
        }
    }
    
    
    void DrawMengerSponge(Vector3 position, int level, int size, string blockName)
    {
        if (level == 0)
        {
            // Dessiner un cube plein à la position donnée
            DrawCube(position, size, blockName);
        }
        else
        {
            int newSize = size / 3;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        if (x != 1 || y != 1 && z != 1) // Ne pas dessiner le cube central et les cubes centraux de chaque face
                        {
                            Vector3 newPosition = new Vector3(position.X + x * newSize, position.Y + y * newSize, position.Z + z * newSize);
                            DrawMengerSponge(newPosition, level - 1, newSize, blockName);
                        }
                    }
                }
            }
        }
    }

    void DrawCube(Vector3 position, int size, string blockName)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    SetBlock(blockName, new Vector3D<int>((int)position.X + x, (int)position.Y + y, (int)position.Z + z));
                }
            }
        }
    }
}
