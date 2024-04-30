using MinecraftCloneSilk.Logger.ChromeEvents;

namespace MinecraftCloneSilk.Logger;

/// <summary>
/// Represents a procedure or session which will be profiled from start to finish.
/// </summary>
public class TraceSession : IDisposable
{
    private readonly string name;
    private readonly long start;
        
    internal string ProcessId { get; }
        
    private bool stopped;
        
    internal TraceSession(string name, string processId)
    {
        this.name = name;
        ProcessId = processId;
        start = ChromeTrace.ElapsedMicroseconds;
        stopped = false;
    }

    /// <summary>
    /// To be called when we the profiling session has ended.
    /// </summary>
    public void Dispose()
    {
        if (!stopped)
            Stop();
    }
        
    private void Stop()
    {
        if (stopped)
            return;

        stopped = true;
        long end =  ChromeTrace.ElapsedMicroseconds;
            
        ChromeEventComplete ev = new ChromeEventComplete(
            name,
            ProcessId,
            start,
            end
        );
            
        ChromeTrace.AddEvent(ev);
    }
}