using System;

namespace KodeRunner
{
    /// <summary>
    /// Map a route path to a Razor view name. The view name is resolved via the MVC View Engine.
    /// Pass view name as e.g. "Sample/Custom" for a file in /Pages/Sample/Custom.cshtml.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal class RazorRouteAttribute : Attribute
    {
        public string Path { get; }
        public string ViewName { get; }
        public string Method { get; }

        public RazorRouteAttribute(string path, string viewName, string method = "GET")
        {
            Path = path;
            ViewName = viewName;
            Method = method;
        }
    }
}