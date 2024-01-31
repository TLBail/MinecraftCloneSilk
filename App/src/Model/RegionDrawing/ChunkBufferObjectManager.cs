using System.Diagnostics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.OpenGL;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.Model.RegionDrawing;

public class ChunkBufferObjectManager : GameObject
{
    private GL gl;

    public List<RegionBuffer> regions = new List<RegionBuffer>();
    private List<RegionBuffer> regionsToUpdate = new List<RegionBuffer>();
    
    public Dictionary<Chunk, RegionBuffer> regionBufferByChunk = new Dictionary<Chunk, RegionBuffer>();
    
    public Stack<RegionBuffer> regionsWithAvailableSpace =  new Stack<RegionBuffer>();
    private Texture cubeTexture;
    private Camera? cam;
    private Game game;
    private Lighting lighting;
    
    public ChunkBufferObjectManager(Game game, Texture cubeTexture) : base(game) {
        this.cubeTexture = cubeTexture;
        this.game = game;
        game.drawables += Draw;
        gl = game.GetGl();
    }


    protected override void Start() {
        cam = game.mainCamera;
        lighting = (game.gameObjects[typeof(World).FullName!] as World)!.lighting;
    }



    public void AddChunkToRegion(Chunk chunk) {
        Debug.Assert(chunk.chunkState >= ChunkState.DRAWABLE, "try to add a chunk with a lower state than the minimum");
        if (regionsWithAvailableSpace.Count == 0) {
            CreateNewRegionBuffer();
        }

        RegionBuffer regionBuffer = regionsWithAvailableSpace.Peek();
        regionBuffer.AddChunk(chunk);
        if(regionBuffer.HaveAvailableSpace()) {
            regionsWithAvailableSpace.Pop();
        }
        regionBufferByChunk.Add(chunk, regionBuffer);
    }
    
    public void NeedToUpdateChunk(Chunk chunk) {
        if(!regionsToUpdate.Contains(regionBufferByChunk[chunk])) regionsToUpdate.Add(regionBufferByChunk[chunk]);
    }

    public void RemoveChunk(Chunk chunk) {
        regionBufferByChunk[chunk].RemoveChunk(chunk);
        NeedToUpdateChunk(chunk);
        if(!regionsWithAvailableSpace.Contains(regionBufferByChunk[chunk])) regionsWithAvailableSpace.Push(regionBufferByChunk[chunk]);
        regionBufferByChunk.Remove(chunk);
    }

    private void Draw(GL gl, double deltatime) {
        foreach (RegionBuffer region in regions) {
            region.Draw(cam!, lighting);
        }
        foreach (RegionBuffer region in regions) {
            region.DrawWater(cam!, lighting);
        }
    }

    [Logger.Timer]
    protected override void Update(double deltatime) {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        while(regionsToUpdate.Count > 0 && stopwatch.ElapsedMilliseconds < 10) {
            regionsToUpdate[0].Update();
            regionsToUpdate.RemoveAt(0);
        }
    }

    
    private void CreateNewRegionBuffer() {
        RegionBuffer region = new RegionBuffer(cubeTexture, gl);
        regionsWithAvailableSpace.Push(region);
        regions.Add(region);
    }


    public override void Destroy() {
        base.Destroy();
        cubeTexture.Dispose();
        foreach (RegionBuffer region in regions) {
            region.Dispose();
        }
    }
}