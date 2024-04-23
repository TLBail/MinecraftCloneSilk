using MinecraftCloneSilk.GameComponent.Components;

namespace MinecraftCloneSilk.GameComponent;

public class BackPack : GameObject
{
    
    public BackPack(Game game) : base(game) {
        ModelRenderer modelRenderer = new ModelRenderer(this, Generated.FilePathConstants.Models.title_fbx);
        components.Add(modelRenderer);
    }
}