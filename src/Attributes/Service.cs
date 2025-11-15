using System;

namespace KodeRunner
{
    /// <summary>
    /// Specifies that a class represents a service component for use with dependency injection or service discovery
    /// mechanisms.
    /// </summary>
    /// <remarks>Apply this attribute to classes that should be recognized as services by frameworks or tools
    /// that support service registration or discovery. This attribute is intended for internal use and may not be
    /// recognized by external frameworks.</remarks>
    [AttributeUsage(AttributeTargets.Class)]
    internal class ServiceAttribute : Attribute
    {
    }
}
