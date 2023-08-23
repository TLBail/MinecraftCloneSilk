using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChromeTracing.NET.ChromeEvents;

namespace ChromeTracing.NET
{
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
        private static ChromeTraceImpl _impl;
        internal static IChromeTracingLogger Logger { get; private set; }


        private static Stopwatch _stopwatch;

        /// <summary>
        /// Chrome trace has microsecond granularity
        /// </summary>
        internal static long ElapsedMicroseconds => _stopwatch.ElapsedMilliseconds * 1000;
        
        
        public static void Init()
        {
            Init(new Logger());
        }
        
        public static void Init(IChromeTracingLogger logger)
        {
            Logger = logger;
            
            _impl = new ChromeTraceImpl();
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            
            Logger.Log("ChromeTracing.NET successfully initialized!");
        }


        public static void SetFileWriter(IFileWriter fileWriter)
        {
            _impl.SetFileWriter(fileWriter);
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
            _stopwatch.Stop();
            // TODO check for running TraceSessions
            _impl.Dispose();
        }
        
        internal static void AddEvent(IChromeEvent ev)
        {
            _impl.AddEvent(ev);
        }



        /// <summary>
        /// Writes the current trace state in a file.
        /// </summary>
        public static void Flush()
        {
            _impl.Flush();
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
        
        /*
        public static void Count(string name, string process = "default")
        {
            AddEvent(new ChromeEventInstant(
                name,
                process,
                ElapsedMicroseconds
            ));
        }
        */
    }
}