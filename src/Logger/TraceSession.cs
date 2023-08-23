using System;
using System.Threading;
using ChromeTracing.NET.ChromeEvents;

namespace ChromeTracing.NET
{
    /// <summary>
    /// Represents a procedure or session which will be profiled from start to finish.
    /// </summary>
    public class TraceSession : IDisposable
    {
        // TODO implement interface to allow profiling single-tick events and nested sessions


        private readonly string _name;
        private readonly long _start;
        
        internal string ProcessId { get; }
        
        private bool _stopped;
        
        internal TraceSession(string name, string processId)
        {
            _name = name;
            ProcessId = processId;
            _start = ChromeTrace.ElapsedMicroseconds;
            _stopped = false;
        }

        /// <summary>
        /// To be called when we the profiling session has ended.
        /// </summary>
        public void Dispose()
        {
            
            ChromeTrace.Logger.Log("Session disposing");
            
            if (!_stopped)
                Stop();
        }
        
        private void Stop()
        {
            if (_stopped)
                return;

            _stopped = true;
            long end =  ChromeTrace.ElapsedMicroseconds;
            
            ChromeEventComplete ev = new ChromeEventComplete(
                _name,
                ProcessId,
                _start,
                end
            );
            
            ChromeTrace.AddEvent(ev);
        }
    }
}