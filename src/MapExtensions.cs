using AsyncMiddleWare.Context;
using AsyncMiddleWare.Utility;
using System;

namespace AsyncMiddleWare
{
    public partial class CallBuilder
    {
        public CallBuilder Map(PathString pathMatch, Action<CallBuilder> configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (pathMatch.HasValue && pathMatch.Value.EndsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException("The path must not end with a '/'", nameof(pathMatch));
            }
            var branchBuilder = New();
            configuration(branchBuilder);
            var branch = branchBuilder.Build();
            var options = new MapOptions
            {
                Branch = branch,
                PathMatch = pathMatch
            };
            return Use(next => new MapMiddleware(next, options).Invoke);
        }
        public CallBuilder MapWhen(Predicate<CallContext> predicate, Action<CallBuilder> configuration)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var branchBuilder = New();
            configuration(branchBuilder);
            var branch = branchBuilder.Build();
            var options = new MapWhenOptions
            {
                Predicate = predicate,
                Branch = branch
            };
            return Use(next => new MapWhenMiddleware(next, options).Invoke);
        }
    }
}
