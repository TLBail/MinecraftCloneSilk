namespace ChromeTracing.NET
{
    internal struct ProfileResult
    {
        public string Name;
        public long Start;
        public long End;
        public string ProcessId;
        public uint ThreadId;
    }
}