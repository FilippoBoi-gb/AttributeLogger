using AspectCore.DynamicProxy;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AttributeLogger
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class LogAttribute : AbstractInterceptorAttribute
    {
        public LogLevel LogLevel { get; set; }

        public LogAttribute(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            var loggerFactory = (ILoggerFactory)context.ServiceProvider.GetService(typeof(ILoggerFactory));
            var logger = loggerFactory.CreateLogger(context.ImplementationMethod.DeclaringType!);

            var method = context.ImplementationMethod;
            var methodName = $"{method.DeclaringType.FullName}.{method.Name}";

            // Skip logging if DisableLogAttribute is present
            if (method.IsDefined(typeof(DisableLogAttribute), true))
            {
                logger.Log(LogLevel, $"Logging is disabled for method {methodName}");

                await next(context);
                return;
            }
            var logParamsAttr = method.GetCustomAttribute<LogParametersAttribute>(true);
            var localLogLevel = logParamsAttr?.LogLevel ?? LogLevel;

            // Create or get invocation GUID
            var invocationGuid = LogContext.GetOrCreateGuid();

            // Log start
            logger.Log(localLogLevel, $"InvocationGuid: [{invocationGuid}] {methodName} Start");

            // Log parameters if LogParametersAttribute is applied
            var disableLog = method.GetCustomAttribute<DisableLogAttribute>(true);
            if (disableLog != null)
            {
                await next(context);
                return;
            }
            if (logParamsAttr != null)
            {
                var parameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramName = parameters[i].Name;
                    var paramValue = context.Parameters.Length > i ? context.Parameters[i] : null;
                    var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(paramValue);
                    logger.Log(localLogLevel,
                        $"InvocationGuid: [{invocationGuid}] {methodName} Param: {paramName} = {serialized}");
                }

            }

            try
            {
                await next(context); // execute the actual method

                logger.Log(localLogLevel, "InvocationGuid: [{Guid}] {Method} End Ok", invocationGuid, methodName);
            }
            catch (Exception ex)
            {

                logger.Log(localLogLevel, "InvocationGuid: [{Guid}] {Method} KO", invocationGuid, methodName);

                LogNestedExceptionsRecursive(logger, ex, invocationGuid, 0);
                if (logParamsAttr?.RaiseExceptions == true)
                    throw;

            }

        }

        private void LogNestedExceptionsRecursive(ILogger logger, Exception ex, Guid guid, int depth)
        {
            if (ex == null || depth > 4) return;

            logger.LogError(ex, $"InvocationGuid: [{guid}] Depth {depth}: {ex.GetType().FullName}: {ex.Message}");

            if (ex.InnerException != null)
            {
                logger.LogError($"InvocationGuid: [{guid}] Inner Exception of type {ex.InnerException.GetType().FullName}");
                LogNestedExceptionsRecursive(logger, ex.InnerException, guid, depth + 1);
            }
        }
    }
}
