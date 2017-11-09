using AsyncMiddleWare.Context;
using AsyncMiddleWare.Logger;
using System;
using System.Threading.Tasks;

namespace AsyncMiddleWare
{
    public class Middleware1 : MiddlewareBase
    {
        public Middleware1(CallDelegate next, ILoggerFactory loggerFactory) : base(next, loggerFactory)
        {
        }

        public override async Task Invoke(CallContext context)
        {
            this._logger.Info("call begin from Middleware1 " + DateTime.Now.ToString("HH:mm:ss"));
            await this._next(context);
            this._logger.Info("call end from Middleware1 " + DateTime.Now.ToString("HH:mm:ss"));
        }
    }

    public class Middleware2 : MiddlewareBase
    {
        public Middleware2(CallDelegate next, ILoggerFactory loggerFactory) : base(next, loggerFactory)
        {
        }

        public override async Task Invoke(CallContext context)
        {
            this._logger.Info("call begin from Middleware2 " + DateTime.Now.ToString("HH:mm:ss"));
            await this._next(context);
            this._logger.Info("call end from Middleware2 " + DateTime.Now.ToString("HH:mm:ss"));
        }
    }

    public class Middleware3 : MiddlewareBase
    {
        public Middleware3(CallDelegate next, ILoggerFactory loggerFactory) : base(next, loggerFactory)
        {
        }

        public override async Task Invoke(CallContext context)
        {
            this._logger.Info("call begin from Middleware3 " + DateTime.Now.ToString("HH:mm:ss"));
            await this._next(context);
            this._logger.Info("call end from Middleware3 " + DateTime.Now.ToString("HH:mm:ss"));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CallBuilder builder = new CallBuilder(new NLoggerFactory());
            builder.UseMiddleware<Middleware1>();
            builder.UseMiddleware<Middleware2>();
            builder.UseMiddleware<Middleware3>();
            builder.Use(async (context, next) =>
           {
               await Console.Out.WriteLineAsync("Call before next.");
               await next(context);
               await Console.Out.WriteLineAsync("Call after next.");
           });
            builder.Run(async (context) =>
           {
               await Console.Out.WriteLineAsync("hello world!");
           });
            var calldelegate = builder.Build();
            var task = calldelegate(new CallContext());
            task.Wait();
        }
    }
}