using System.Diagnostics;
using MinecraftCloneSilk.Logger.ChromeEvents;

namespace MinecraftCloneSilk.Logger;
/*
    Info about the file format at
    https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU/preview
    
    More info about the profiling tool at
    https://www.chromium.org/developers/how-tos/trace-event-profiling-tool
 */

/// <summary>
/// API that outputs a .json file to be displayed by
/// Google Chrome's chrome://tracing tool.
/// </summary>
public static class ChromeTrace
{
    private static ChromeTraceImpl? impl;
    internal static IChromeTracingLogger? Logger { get; private set; }


    private static Stopwatch? stopwatch;

    /// <summary>
    /// Chrome trace has microsecond granularity
    /// </summary>
    internal static long ElapsedMicroseconds => stopwatch is not null ? stopwatch.ElapsedMilliseconds * 1000 : 0;
        
        
    public static void Init()
    {
        Init(new Logger());
    }
        
    public static void Init(IChromeTracingLogger? logger)
    {
        Logger = logger;
            
        impl = new ChromeTraceImpl();
        stopwatch = new Stopwatch();
        stopwatch.Start();
            
        Logger?.Log("ChromeTracing.NET successfully initialized!");
    }


    public static void SetFileWriter(IFileWriter fileWriter)
    {
        impl?.SetFileWriter(fileWriter);
    }
        
        

    /// <summary>
    /// <para>
    /// To be called when we want ChromeTrace.NET to shut down and
    /// output a profiling JSON file.
    /// </para>
    /// 
    /// <para>
    /// Can be called at the end of the application execution, even
    /// though it's not always necessary: <see cref="ChromeTrace"/> will
    /// automatically dispose with the last pass of the Garbage Collector.
    /// </para>
    /// </summary>
    public static void Dispose()
    {
        stopwatch?.Stop();
        impl?.Dispose();
    }
        
    internal static void AddEvent(IChromeEvent ev)
    {
        if(impl is not null)
            impl.AddEvent(ev);
    }



    /// <summary>
    /// Writes the current trace state in a file.
    /// </summary>
    public static void Flush()
    {
        if(impl is not null)
            impl.Flush();
    }
        
        
        
    public static void BeginTrace(string name, string process = "default")
    {
        AddEvent(new ChromeEventDuration(
            name,
            process,
            ElapsedMicroseconds,
            'B'
        ));
    }
        
    public static void EndTrace(string name, string process = "default")
    {
        AddEvent(new ChromeEventDuration(
            name,
            process,
            ElapsedMicroseconds,
            'E'
        ));
    }
        
        
        
    /// <summary>
    /// Opens a <see cref="TraceSession"/>, which can be later disposed.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="process"></param>
    /// <returns></returns>
    public static TraceSession Profile(string name, string process = "default")
    {
        return new TraceSession(name, process);
    }


    public static void Instant(string name, string process = "default")
    {
        AddEvent(new ChromeEventInstant(
            name,
            process,
            ElapsedMicroseconds
        ));
    }

}