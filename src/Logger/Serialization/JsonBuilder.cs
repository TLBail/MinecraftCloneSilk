using System.Collections.Generic;
using System.Text;

namespace ChromeTracing.NET.Serialization
{
    /// <summary>
    /// Super easy, straight-forward JSON serialization for
    /// objects composed of key-value pairs.
    /// </summary>
    internal class JsonBuilder
    {
        private bool _busy;
        private StringBuilder _str;

        public JsonBuilder()
        {
            _str = new StringBuilder();
        }

        private void Append(string str)
        {
            string c = _busy ? ", " : "{ ";
            _str.Append(c);
            _str.Append(str);
            _busy = true;
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
            _str.Append("}");
            return _str.ToString();
        }
    }
}