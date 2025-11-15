using System;

namespace KodeRunner
{
    /// <summary>
    /// Specifies the route path and HTTP method for an action method in a web application.
    /// </summary>
    /// <remarks>Apply this attribute to a method to define the URL path and HTTP verb that the method should
    /// handle. This is typically used in custom routing scenarios to map incoming HTTP requests to specific action
    /// methods based on their path and method. The default HTTP method is "GET" if not specified.</remarks>
    [AttributeUsage(AttributeTargets.Method)]
    internal class RouteAttribute : Attribute
    {
        public string Path { get; }
        public string Method { get; }

        public RouteAttribute(string path, string method = "GET")
        {
            Path = path;
            Method = method;
        }
    }
}