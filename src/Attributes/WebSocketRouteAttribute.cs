using System;

namespace KodeRunner
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class WebSocketRouteAttribute : Attribute
    {
        public string Path { get; }

        public WebSocketRouteAttribute(string path)
        {
            Path = path;
        }
    }
}