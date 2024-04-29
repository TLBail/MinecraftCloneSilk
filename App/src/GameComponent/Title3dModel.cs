using System.Numerics;
using MinecraftCloneSilk.GameComponent.Components;

namespace MinecraftCloneSilk.GameComponent;

public class Title3dModel : GameObject
{
    
    private Player player;
    private ModelRenderer modelRenderer;
    private double time = 0;
    
   
    public Title3dModel(Game game) : base(game) {
        modelRenderer = new ModelRenderer(this, Generated.FilePathConstants.Models.title_fbx);
        components.Add(modelRenderer);
    }

    
    
    protected override void Start() {
        player = (Player)game.gameObjects[typeof(Player).FullName!];
        
    }
    

    protected override void Update(double deltaTime) {
        time += deltaTime;
        //rotate around the player
        modelRenderer.transform.Position = new Vector3(
            player.position.X + (100 * (float)Math.Cos(time)),
            player.position.Y + 10,
            player.position.Z + (100 * (float)Math.Sin(time))
        );
        
    }
}