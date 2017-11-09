using System;

namespace AsyncMiddleWare.Logger
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(Type type);
    }
}
