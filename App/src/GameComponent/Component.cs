namespace MinecraftCloneSilk.GameComponent;

public abstract class Component
{

    public GameObject gameObject;

    public Component(GameObject gameObject) {
        this.gameObject = gameObject;
    }
    
    
    public virtual void Start(){}

    public virtual void Destroy() { }

    public virtual void ToImGui() { }
}