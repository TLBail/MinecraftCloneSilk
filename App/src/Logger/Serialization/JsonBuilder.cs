using System.Text;

namespace MinecraftCloneSilk.Logger.Serialization;

/// <summary>
/// Super easy, straight-forward JSON serialization for
/// objects composed of key-value pairs.
/// </summary>
internal class JsonBuilder
{
    private bool busy;
    private StringBuilder str;

    public JsonBuilder()
    {
        str = new StringBuilder();
    }

    private void Append(string str)
    {
        string c = busy ? ", " : "{ ";
        this.str.Append(c);
        this.str.Append(str);
        busy = true;
    }
        
        

    public void Add(string key, string value)
    {
        Append($"\"{key}\" : \"{value}\"");
    }
        
    public void Add(string key, char value)
    {
        Append($"\"{key}\" : \"{value}\"");
    }
        
    public void Add(string key, long value)
    {
        Append($"\"{key}\" : {value}");
    }

    public string Build()
    {
        str.Append("}");
        return str.ToString();
    }
}