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
    public int radius { get; set; } = 12;
    private readonly WorldUi worldUi;
    public IWorldGenerator worldGeneration;
    public WorldMode worldMode { get; set; }
    public ChunkManager chunkManager;
    private IChunkStorage chunkStorage;
    public Lighting lighting { get; private set; }
    private List<string> commandKeys = new();
    
    public World(Game game, WorldMode worldMode) : this(game, null, worldMode, null){} 
    public World(Game game, IWorldGenerator worldGeneration) : this(game, worldGeneration, WorldMode.EMPTY){}
    public World(Game game, WorldMode worldMode, string? saveLocation) : this(game, null, worldMode, saveLocation){}
    public World(Game game, IWorldGenerator worldGeneration, WorldMode worldMode) : this(game, worldGeneration, worldMode, null){}

    public World(Game game, IWorldGenerator? worldGenerator = null, WorldMode worldMode = WorldMode.EMPTY, string? saveLocation = null) : base(game) {
        this.worldMode = worldMode;
        this.lighting = new Lighting();
        worldUi = new WorldUi(this, lighting);
        worldGeneration = worldGenerator ?? new WorldNaturalGeneration();
        chunkStorage = saveLocation is not null ? new RegionStorage(saveLocation) : new NullChunkStorage();
        chunkManager = new ChunkManager(radius, worldGeneration, chunkStorage);
    } 
    
    public void Reset(IWorldGenerator? worldGenerator = null,  WorldMode worldMode = WorldMode.EMPTY, string? saveLocation = null) {
        chunkManager.Clear();
        chunkStorage.Dispose();
        
        worldGeneration = worldGenerator ?? new WorldNaturalGeneration();
        chunkStorage = saveLocation is not null ? new RegionStorage(saveLocation) : new NullChunkStorage();
        chunkManager = new ChunkManager(radius, worldGeneration, chunkStorage);
        SetWorldMode(worldMode);
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
            chunkManager.LoadChunkAroundACenter(GetChunkPosition(new Vector3D<int>((int)player.position.X, (int)player.position.Y, (int)player.position.Z)), radius);
        }
        chunkManager.Update(deltaTime);
    }

    public override void Destroy() {
        base.Destroy();
        foreach(string key in commandKeys) console.RemoveCommand(key);
        chunkStorage.Dispose();
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
                chunkManager.LoadChunkAroundACenter(GetChunkPosition(new Vector3D<int>((int)player.position.X, (int)player.position.Y, (int)player.position.Z)), radius);
                break;
        }
    }

    public ICollection<Chunk> GetWorldChunks() {
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
        commandKeys.Add("/addChunk");
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

        commandKeys.Add("/rmChunk");
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

        commandKeys.Add("/setBlock");
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

        commandKeys.Add("/getBlock");
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

        commandKeys.Add("/getChunk");
        console.AddCommand("/getChunk", (commandParams) =>
        {
            if (commandParams.Length >= 3) {
                Vector3D<int> position = new Vector3D<int>(
                    int.Parse(commandParams[0]),
                    int.Parse(commandParams[1]),
                    int.Parse(commandParams[2])
                );
                console.Log("chunk at " + position + " is \n" + GetChunk(position).ToString());
            } else {
                //print the chunk where the player is 
                Vector3D<int> position = new Vector3D<int>(
                    (int)player.position.X,
                    (int)player.position.Y,
                    (int)player.position.Z
                );
                console.Log("chunk at " + position + " is \n" + GetChunk(position).ToString());

            }
        });
        
        commandKeys.Add("/blockLine");
        console.AddCommand("/blockLine", (commandParams) =>
        {
            if (commandParams.Length == 2) {
                Brush.Line(this,
                    player,
                    int.Parse(commandParams[0]),
                    commandParams[1]);
            } else {
                console.Log("require the number of block and the block name");
            }
        });
        
        commandKeys.Add("/bomb");
        console.AddCommand("/bomb", (commandParams) =>
        {
            int size = 10;
            if (commandParams.Length >= 1) size = int.Parse(commandParams[0]);
            Brush.Bomb(this,
                new Vector3D<int>(
                    (int)player.position.X,
                    (int)player.position.Y,
                    (int)player.position.Z
                ), size);
        });
        
        commandKeys.Add("/wall");
        console.AddCommand("/wall", (commandParams) =>
        {
            if (commandParams.Length == 2) {
                int size = int.Parse(commandParams[0]);
                string blockName = commandParams[1];
                Brush.Wall(this,
                    player,
                    size,
                    blockName);
            }else {
                console.Log("require the size and the block name");
            }
        });
        
        commandKeys.Add("/spiral");
        console.AddCommand("/spiral", (commandParams) =>
        {
            if (commandParams.Length == 3)
            {
                int nbTurns = int.Parse(commandParams[0]); // Nombre de tours de la spirale
                string blockName = commandParams[1]; // Nom du bloc à utiliser pour la spirale
                float heightIncrement = float.Parse(commandParams[2]); // Incrément de hauteur après chaque tour complet
                Brush.Spiral(this,
                    player.position,
                    nbTurns,
                    heightIncrement,
                    blockName);
            }
            else
            {
                console.Log("require the number of turns, block name, and height increment");
            }
        });

        
        commandKeys.Add("/sierpinski");
        console.AddCommand("/sierpinski", (commandParams) =>
        {
            if (commandParams.Length == 3)
            {
                int level = int.Parse(commandParams[0]); // Niveau de récursivité du fractal
                string blockName = commandParams[1]; // Nom du bloc à utiliser
                int size = int.Parse(commandParams[2]); // Taille de base du triangle
                Brush.Sierpinski(this,player.position, level, size, blockName);
            }
            else
            {
                console.Log("require the recursion level, block name, and size");
            }
        });
        
        commandKeys.Add("/mengerSponge");
        console.AddCommand("/mengerSponge", (commandParams) =>
        {
            if (commandParams.Length == 3)
            {
                int level = int.Parse(commandParams[0]); // Niveau de récursivité du fractal
                string blockName = commandParams[1]; // Nom du bloc à utiliser
                int size = int.Parse(commandParams[2]); // Taille initiale du cube
                Brush.MengerSponge(this,player.position, level, size, blockName);
            }
            else
            {
                console.Log("require the recursion level, block name, and size");
            }
        });
        
      
    }



    private void AddExempleChunk() {
        Vector3D<int>[] postions =
        {
            Vector3D<int>.Zero,
        };
        chunkManager.AddChunksToLoad(postions.ToList());
    }

    public override void ToImGui() {
        worldUi.DrawUi();
    }

}
