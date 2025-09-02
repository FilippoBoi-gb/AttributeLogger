# AttributeLogger
1. [Purpose](mMain-purpose)
2. [Attributes](#attributes)
3. [Usage](#usage)


This library provides a simple Aspect-Oriented Programming(AOP) approach to logging of C# classes and methods, using AspectCore.DynamicProxy to intercept method calls and integrates with Microsoft.Extensions.Logging.
It automatically logs nested exceptions and propagates them by default.
The standard logging info is formatted as follow

>InvocationGuid: [f5c7e3c2-0d4f-45e8-9a2d-123456789abc] MyApp.Services.OrderService.CreateOrder Start
>
>InvocationGuid: [f5c7e3c2-0d4f-45e8-9a2d-123456789abc] MyApp.Services.OrderService.CreateOrder Param firstParam: ""
>
>InvocationGuid: [f5c7e3c2-0d4f-45e8-9a2d-123456789abc] MyApp.Services.OrderService.CreateOrder End

## Main Purpose
The main purpose is to:
- Automatically log method start, end, and failures.
- Allow flexible logging levels (Trace, Debug, Information, Warning, Error, Critical).
- Enable or disable logging at the method level.
- Optionally log method parameters.
- Correlate logs across method calls using a correlation GUID.

## Attributes

### LogAttribute
This is the beating heart of this library. It's responsable of the whole logging system and specifies the logging level.

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
 public class LogAttribute : AbstractInterceptorAttribute
 {
     public LogLevel LogLevel { get; set; }

     public LogAttribute(LogLevel logLevel)
     {
         LogLevel = logLevel;
     }
}

### DisableLogAttribute

this method avoids unwanted methods logging, do not need further parameters.

` [AttributeUsage(AttributeTargets.Method, Inherited = false)]
  public class DisableLogAttribute : Attribute
  {
      // This attribute is used to disable logging for specific methods
  }
`

### LogParametersAttribute

This method enables method parameters input logging, to a specific level. Also allows to handle exception propagation

` [AttributeUsage(AttributeTargets.Method, Inherited = false)]
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
 }`

## Usage

1. Decorate Classes

`[Log(LogLevel.Information)]
public class MyServiceService
{
    public void CreateOrder(string customerId)
    {
        // Logs automatically
    }
}`
this will create the following logging

>InvocationGuid: [f5c7e3c2-0d4f-45e8-9a2d-123456789abc] MyApp.Services.MyServiceService.CreateOrder Start
>InvocationGuid: [f5c7e3c2-0d4f-45e8-9a2d-123456789abc] MyApp.Services.MyServiceService.CreateOrder End

2. Decorate Methods (Optional)
   
[Log(LogLevel.Information)]
 public class MyServiceService
 {
     public void CreateOrder(string customerId)
     {
        MyCommand.CreateOrder(customerId);
     }
     
      public void CreateOrder(string OrderId)
     {
         // Logs automatically
     }
 }

[Log(LogLevel.Information)]
public class MyCommand(){
 
    [DisableLog]
    public CreateOrder(string customerId){
       WriteToDatabase(customerId);
     }   
}



Register AspectCore
In your Program.cs or Startup.cs:
`services.AddDynamicProxy(); // Enables AspectCore interception`

