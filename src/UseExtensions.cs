using AsyncMiddleWare.Context;
using AsyncMiddleWare.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncMiddleWare
{
    public delegate Task CallDelegate(CallContext context);
    public partial class CallBuilder
    {
        public CallBuilder UseMiddleware<T>()
        {
            var type = typeof(T);

            if (!typeof(MiddlewareBase).IsAssignableFrom(type))
                throw new ArgumentException($"{type.Name}不是有效的MiddlewareBase类型");
            return this.Use((next) =>
            {
                var constructor = type.GetConstructor(new[] { typeof(CallDelegate), typeof(ILoggerFactory) });
                var middleware = (MiddlewareBase)constructor.Invoke(new object[] { next, loggerFactory });
                return middleware.Invoke;
            });
        }

        public CallBuilder Use(Func<CallDelegate, CallDelegate> fun)
        {
            middlewareList.Add(fun);
            return this;
        }

        public CallBuilder Use(Func<CallContext, CallDelegate, Task> fun)
        {
            Func<CallDelegate, CallDelegate> func = (next) =>
             {
                 return new CallDelegate((context) =>
                 {
                     return fun(context, next);
                 });
             };
            return this.Use(func);
        }

        public CallBuilder Run(Func<CallContext, Task> last)
        {
            Func<CallDelegate, CallDelegate> func = (next) =>
              {
                  return new CallDelegate(last);
              };
            return this.Use(func);
        }

        public CallBuilder New()
        {
            return new CallBuilder(loggerFactory);
        }
    }
}
