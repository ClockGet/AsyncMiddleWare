using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncMiddleWare
{
    public class CallContext
    {
    }
    public interface ILogger
    {
        Type AttachParent { get; }

        void Log(string message);
    }
    public interface ILoggerFactory
    {
        ILogger CreateLogger(Type type);
    }

    public class Logger : ILogger
    {
        public Type AttachParent
        {
            get;
            internal set;
        }

        public void Log(string message)
        {
            Console.WriteLine("[info] from class:" + AttachParent.FullName + "\r\n" + message);
        }
    }

    public class LoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(Type type)
        {
            return new Logger { AttachParent = type };
        }
    }

    public delegate Task CallDelegate(CallContext context);

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

    public class CallBuilder
    {
        private List<Func<CallDelegate, CallDelegate>> middlewareList = new List<Func<CallDelegate, CallDelegate>>();
        private static Task CompletedTask = Task.FromResult(false);
        private CallDelegate _last = (context) =>
         {
             return CompletedTask;
         };
        private ILoggerFactory factory = null;
        public CallBuilder(ILoggerFactory loggerFactory)
        {
            factory = loggerFactory;
        }

        public CallBuilder UseMiddleware<T>()
        {
            var type = typeof(T);

            if (!typeof(MiddlewareBase).IsAssignableFrom(type))
                throw new ArgumentException($"{type.Name}不是有效的MiddlewareBase类型");
            return this.Use((next) =>
            {
                var constructor = type.GetConstructor(new[] { typeof(CallDelegate), typeof(ILoggerFactory) });
                var middleware = (MiddlewareBase)constructor.Invoke(new object[] { next, factory });
                return middleware.Invoke;
            });
        }

        public CallBuilder Use(Func<CallDelegate, CallDelegate> fun)
        {
            middlewareList.Add(fun);
            return this;
        }

        public CallBuilder Use(Func<CallContext, Task> last)
        {
            Func<CallDelegate, CallDelegate> func = (next) =>
              {
                  return new CallDelegate(last);
              };
            return this.Use(func);
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

        public CallDelegate Build()
        {
            int len = middlewareList.Count;

            if (len == 0)
                return null;

            CallDelegate callDelegate = null;

            if (len == 1)
            {
                callDelegate = middlewareList[0].Invoke(_last);
            }
            else
            {
                CallDelegate next = middlewareList[len - 1].Invoke(_last);

                for (int i = len - 2; i > -1; i--)
                {
                    next = middlewareList[i].Invoke(next);
                }
                callDelegate = next;
            }
            return callDelegate;
        }
    }

    public class Middleware1 : MiddlewareBase
    {
        public Middleware1(CallDelegate next, ILoggerFactory loggerFactory) : base(next, loggerFactory)
        {
        }

        public override async Task Invoke(CallContext context)
        {
            this._logger.Log("call begin from Middleware1 " + DateTime.Now.ToString("HH:mm:ss"));
            await this._next(context);
            this._logger.Log("call end from Middleware1 " + DateTime.Now.ToString("HH:mm:ss"));
        }
    }

    public class Middleware2 : MiddlewareBase
    {
        public Middleware2(CallDelegate next, ILoggerFactory loggerFactory) : base(next, loggerFactory)
        {
        }

        public override async Task Invoke(CallContext context)
        {
            this._logger.Log("call begin from Middleware2 " + DateTime.Now.ToString("HH:mm:ss"));
            await this._next(context);
            this._logger.Log("call end from Middleware2 " + DateTime.Now.ToString("HH:mm:ss"));
        }
    }

    public class Middleware3 : MiddlewareBase
    {
        public Middleware3(CallDelegate next, ILoggerFactory loggerFactory) : base(next, loggerFactory)
        {
        }

        public override async Task Invoke(CallContext context)
        {
            this._logger.Log("call begin from Middleware3 " + DateTime.Now.ToString("HH:mm:ss"));
            await this._next(context);
            this._logger.Log("call end from Middleware3 " + DateTime.Now.ToString("HH:mm:ss"));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CallBuilder builder = new CallBuilder(new LoggerFactory());
            builder.UseMiddleware<Middleware1>();
            builder.UseMiddleware<Middleware2>();
            builder.UseMiddleware<Middleware3>();
            builder.Use(async (context, next) =>
           {
               await Console.Out.WriteLineAsync("Call before next.");
               await next(context);
               await Console.Out.WriteLineAsync("Call after next.");
           });
            builder.Use(async (context) =>
           {
               await Console.Out.WriteLineAsync("hello world!");
           });
            var calldelegate = builder.Build();
            var task = calldelegate(new CallContext());
            task.Wait();
        }
    }
}