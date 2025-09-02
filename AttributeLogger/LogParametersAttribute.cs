using Microsoft.Extensions.Logging;

namespace AttributeLogger
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class LogParametersAttribute : Attribute
    {
        public LogLevel LogLevel { get; set; }
        public bool RaiseExceptions { get; set; }
        public LogParametersAttribute() { }
        public LogParametersAttribute(LogLevel logLevel = LogLevel.Information, bool raiseExceptions = false)
        {
            LogLevel = logLevel;
            RaiseExceptions = raiseExceptions;
        }
    }
}
