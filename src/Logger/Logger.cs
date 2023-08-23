using System;

namespace ChromeTracing.NET
{
    /// <summary>
    /// Class used to immediately prompt messages to the user.
    /// Right now, using <see cref="System.Console"/>, which
    /// should be replaced in the future by a proper logging
    /// library.
    /// </summary>
    internal class Logger : IChromeTracingLogger
    {
        public void Log(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}