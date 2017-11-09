using AsyncMiddleWare.Context;
using AsyncMiddleWare.Utility;
using System;
using System.Threading.Tasks;

namespace AsyncMiddleWare.Utility
{
    internal class MapMiddleware
    {
        protected readonly CallDelegate _next;
        protected readonly MapOptions _options;
        public MapMiddleware(CallDelegate next, MapOptions options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            this._next = next;
            this._options = options;
        }
        public async Task Invoke(CallContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            PathString path = context.Path;
            if (path.StartsWithSegments(_options.PathMatch))
            {
                try
                {
                    await _options.Branch(context);
                }
                catch
                {

                }
            }
            else
            {
                await _next(context);
            }
        }
    }
    internal class MapWhenMiddleware
    {
        protected readonly CallDelegate _next;
        protected readonly MapWhenOptions _options;
        public MapWhenMiddleware(CallDelegate next, MapWhenOptions options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            this._next = next;
            this._options = options;
        }
        public async Task Invoke(CallContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (_options.Predicate(context))
            {
                await _options.Branch(context);
            }
            else
            {
                await _next(context);
            }
        }
    }
}
