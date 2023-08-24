using MinecraftCloneSilk.GameComponent;

namespace MinecraftCloneSilk.Model;

public class GameException : Exception
{
    public GameObject? gameObject { get; set; }

    public GameException(GameObject? gameObject, string message) : base(message) {
        this.gameObject = gameObject;
    }
    
    public GameException(string message) :this(null, message){}

    public override string ToString() {
        if (gameObject != null) {
            return gameObject.ToString() + " : " + Message;
        }
        return Message;
    }
}