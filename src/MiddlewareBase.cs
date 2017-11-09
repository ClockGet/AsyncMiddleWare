using AsyncMiddleWare.Context;
using AsyncMiddleWare.Logger;
using System.Threading.Tasks;

namespace AsyncMiddleWare
{
    public abstract class MiddlewareBase
    {
        protected readonly CallDelegate _next;
        protected readonly ILogger _logger;
        public MiddlewareBase(CallDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger(this.GetType());
        }

        public abstract Task Invoke(CallContext context);
    }
}
