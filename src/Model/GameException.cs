using MinecraftCloneSilk.GameComponent;

namespace MinecraftCloneSilk.Model;

public class GameException : Exception
{
    public GameObject gameObject { get; set; }

    public GameException(GameObject gameObject, string message) : base(message) {
        this.gameObject = gameObject;
    }

    public override string ToString() {
        return gameObject.ToString() + " : " + Message;
    }
}