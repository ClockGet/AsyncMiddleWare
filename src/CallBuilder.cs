using AsyncMiddleWare.Logger;
using AsyncMiddleWare.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncMiddleWare
{
    public sealed partial class CallBuilder
    {
        private IList<Func<CallDelegate, CallDelegate>> middlewareList = new List<Func<CallDelegate, CallDelegate>>();
        private static Task CompletedTask = Task.FromResult(false);
        private CallDelegate _last = (context) =>
         {
             return CompletedTask;
         };
        private ILoggerFactory loggerFactory = null;
        public CallBuilder(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }
        public CallDelegate Build()
        {
            CallDelegate callDelegate = _last;
            foreach(var m in middlewareList.Reverse())
            {
                callDelegate = m(callDelegate);
            }
            return CallProxy = callDelegate;
        }
        public CallDelegate CallProxy
        {
            get;
            private set;
        }
        public ILoggerFactory LoggerFactory
        {
            get
            {
                return loggerFactory;
            }
        }
    }
}
