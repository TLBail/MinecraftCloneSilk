using System;
using System.IO;

namespace ChromeTracing.NET.Serialization
{
    public class PcFileWriter : IFileWriter
    {
        public void Write(string content, string filename)
        {
            string path = Path.Combine(Environment.CurrentDirectory, filename);
            File.WriteAllText(path, content);
            
            ChromeTrace.Logger.Log("ChromeTracing.NET trace file created: " + path);
        }

        public void WriteTemp(string content, string filename)
        {
            string path = Path.Combine(Environment.CurrentDirectory, filename);
            File.WriteAllText(path, content);
            
            ChromeTrace.Logger.Log("ChromeTracing.NET trace file created: " + path);
        }
    }
}