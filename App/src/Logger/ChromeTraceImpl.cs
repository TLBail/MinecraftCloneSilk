using System.Text;
using MinecraftCloneSilk.Logger.Serialization;

namespace MinecraftCloneSilk.Logger;

/// <summary>
/// Actual implementation of the <see cref="ChromeTrace"/> API.
/// </summary>
internal class ChromeTraceImpl
{
    private readonly List<IChromeEvent> results;
        
    private IFileWriter fileWriter;
        

    public ChromeTraceImpl()
    {
        results = new List<IChromeEvent>();
        fileWriter = new PcFileWriter();
    }

    ~ChromeTraceImpl()
    {
        Dispose();
    }
        
    public void SetFileWriter(IFileWriter fileWriter)
    {
        this.fileWriter = fileWriter;
    }
        
        
    public void AddEvent(IChromeEvent ev)
    {
        results.Add(ev);
    }



    public void Flush()
    {
        DateTime dt = DateTime.Now;
        string str = dt.ToLongTimeString().Replace(':', '_');
        fileWriter.WriteTemp(Write(), "flush_" + str + ".json");
    }
        

    public void Dispose()
    {
        ChromeTrace.Logger?.Log("ChromeTracing.NET disposing...");
        fileWriter.Write(Write(), "trace.json");
    }


    private string Write()
    {
        StringBuilder str = new StringBuilder();

        str.Append(WriteHeader());
            
        if (results.Count > 0)
        {
            for (int i = 0; i < results.Count - 1; i++)
            {
                str.Append(WriteProfile(results[i]));
                str.Append(",\n");
            }
            str.Append(WriteProfile(results[results.Count-1]));
        }
            
        str.Append(WriteFooter());
        return str.ToString();
    }
        
        
        
        
        
        
        
        
    // Info about the format
    // https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU/preview
        
        
    private static string WriteHeader()
    {
        return "{\"otherData\": {}, \"traceEvents\":[\n";
    }

    private static string WriteProfile(IChromeEvent ev)
    {
        return ev.Serialize();
    }
        
    private static string WriteFooter()
    {
        //return "\n],\n\"displayTimeUnit\": \"ms\"}";
        return "\n]}";
    }
        
}