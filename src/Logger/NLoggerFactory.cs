using System;

namespace AsyncMiddleWare.Logger
{
    public sealed class NLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(Type type)
        {
            return new NLogger() { AttachParent = type };
        }
    }
}
