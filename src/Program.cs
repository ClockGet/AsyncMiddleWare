using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AsyncMiddleWare
{
    #region PathString Copy from https://github.com/aspnet/HttpAbstractions
    internal class PathStringHelper
    {
        private static bool[] ValidPathChars = {
            false, false, false, false, false, false, false, false,     // 0x00 - 0x07
            false, false, false, false, false, false, false, false,     // 0x08 - 0x0F
            false, false, false, false, false, false, false, false,     // 0x10 - 0x17
            false, false, false, false, false, false, false, false,     // 0x18 - 0x1F
            false, true,  false, false, true,  false, true,  true,      // 0x20 - 0x27
            true,  true,  true,  true,  true,  true,  true,  true,      // 0x28 - 0x2F
            true,  true,  true,  true,  true,  true,  true,  true,      // 0x30 - 0x37
            true,  true,  true,  true,  false, true,  false, false,     // 0x38 - 0x3F
            true,  true,  true,  true,  true,  true,  true,  true,      // 0x40 - 0x47
            true,  true,  true,  true,  true,  true,  true,  true,      // 0x48 - 0x4F
            true,  true,  true,  true,  true,  true,  true,  true,      // 0x50 - 0x57
            true,  true,  true,  false, false, false, false, true,      // 0x58 - 0x5F
            false, true,  true,  true,  true,  true,  true,  true,      // 0x60 - 0x67
            true,  true,  true,  true,  true,  true,  true,  true,      // 0x68 - 0x6F
            true,  true,  true,  true,  true,  true,  true,  true,      // 0x70 - 0x77
            true,  true,  true,  false, false, false, true,  false,     // 0x78 - 0x7F
        };

        public static bool IsValidPathChar(char c)
        {
            return c < ValidPathChars.Length && ValidPathChars[c];
        }

        public static bool IsPercentEncodedChar(string str, int index)
        {
            return index < str.Length - 2
                && str[index] == '%'
                && IsHexadecimalChar(str[index + 1])
                && IsHexadecimalChar(str[index + 2]);
        }

        public static bool IsHexadecimalChar(char c)
        {
            return ('0' <= c && c <= '9')
                || ('A' <= c && c <= 'F')
                || ('a' <= c && c <= 'f');
        }
    }
    public struct PathString : IEquatable<PathString>
    {
        private static readonly PathString Empty = new PathString(string.Empty);
        private readonly string _value;
        public PathString(string value)
        {
            if(!string.IsNullOrEmpty(value) && value[0]!='/')
            {
                throw new ArgumentException("Path must start with slash", nameof(value));
            }
            _value = value;
        }
        public string Value
        {
            get
            {
                return _value;
            }
        }
        public bool HasValue
        {
            get
            {
                return !string.IsNullOrEmpty(_value);
            }
        }
        public override string ToString()
        {
            return ToUriComponent();
        }
        public string ToUriComponent()
        {
            if (!HasValue)
            {
                return string.Empty;
            }

            StringBuilder buffer = null;

            var start = 0;
            var count = 0;
            var requiresEscaping = false;
            var i = 0;

            while (i < _value.Length)
            {
                var isPercentEncodedChar = PathStringHelper.IsPercentEncodedChar(_value, i);
                if (PathStringHelper.IsValidPathChar(_value[i]) || isPercentEncodedChar)
                {
                    if (requiresEscaping)
                    {
                        // the current segment requires escape
                        if (buffer == null)
                        {
                            buffer = new StringBuilder(_value.Length * 3);
                        }

                        buffer.Append(Uri.EscapeDataString(_value.Substring(start, count)));

                        requiresEscaping = false;
                        start = i;
                        count = 0;
                    }

                    if (isPercentEncodedChar)
                    {
                        count += 3;
                        i += 3;
                    }
                    else
                    {
                        count++;
                        i++;
                    }
                }
                else
                {
                    if (!requiresEscaping)
                    {
                        // the current segument doesn't require escape
                        if (buffer == null)
                        {
                            buffer = new StringBuilder(_value.Length * 3);
                        }

                        buffer.Append(_value, start, count);

                        requiresEscaping = true;
                        start = i;
                        count = 0;
                    }

                    count++;
                    i++;
                }
            }

            if (count == _value.Length && !requiresEscaping)
            {
                return _value;
            }
            else
            {
                if (count > 0)
                {
                    if (buffer == null)
                    {
                        buffer = new StringBuilder(_value.Length * 3);
                    }

                    if (requiresEscaping)
                    {
                        buffer.Append(Uri.EscapeDataString(_value.Substring(start, count)));
                    }
                    else
                    {
                        buffer.Append(_value, start, count);
                    }
                }

                return buffer.ToString();
            }
        }
        public static PathString FromUriComponent(string uriComponent)
        {
            return new PathString(Uri.UnescapeDataString(uriComponent));
        }
        public static PathString FromUriComponent(Uri uri)
        {
            if(uri==null)
            {
                throw new ArgumentNullException(nameof(uri));
            }
            return new PathString("/" + uri.GetComponents(UriComponents.Path, UriFormat.Unescaped));
        }
        public bool StartsWithSegments(PathString other)
        {
            return StartsWithSegments(other, StringComparison.OrdinalIgnoreCase);
        }
        public bool StartsWithSegments(PathString other, StringComparison comparisonType)
        {
            var value1 = Value ?? string.Empty;
            var value2 = other.Value ?? string.Empty;
            if (value1.StartsWith(value2, comparisonType))
            {
                return value1.Length == value2.Length || value1[value2.Length] == '/';
            }
            return false;
        }
        public bool StartsWithSegments(PathString other, out PathString remaining)
        {
            return StartsWithSegments(other, StringComparison.OrdinalIgnoreCase, out remaining);
        }
        public bool StartsWithSegments(PathString other, StringComparison comparisonType, out PathString remaining)
        {
            var value1 = Value ?? string.Empty;
            var value2 = other.Value ?? string.Empty;
            if (value1.StartsWith(value2, comparisonType))
            {
                if (value1.Length == value2.Length || value1[value2.Length] == '/')
                {
                    remaining = new PathString(value1.Substring(value2.Length));
                    return true;
                }
            }
            remaining = Empty;
            return false;
        }
        public bool StartsWithSegments(PathString other, out PathString matched, out PathString remaining)
        {
            return StartsWithSegments(other, StringComparison.OrdinalIgnoreCase, out matched, out remaining);
        }
        public bool StartsWithSegments(PathString other, StringComparison comparisonType, out PathString matched, out PathString remaining)
        {
            var value1 = Value ?? string.Empty;
            var value2 = other.Value ?? string.Empty;
            if (value1.StartsWith(value2, comparisonType))
            {
                if (value1.Length == value2.Length || value1[value2.Length] == '/')
                {
                    matched = new PathString(value1.Substring(0, value2.Length));
                    remaining = new PathString(value1.Substring(value2.Length));
                    return true;
                }
            }
            remaining = Empty;
            matched = Empty;
            return false;
        }
        public PathString Add(PathString other)
        {
            if (HasValue &&
                other.HasValue &&
                Value[Value.Length - 1] == '/')
            {
                // If the path string has a trailing slash and the other string has a leading slash, we need
                // to trim one of them.
                return new PathString(Value + other.Value.Substring(1));
            }

            return new PathString(Value + other.Value);
        }
        public bool Equals(PathString other)
        {
            return Equals(other, StringComparison.OrdinalIgnoreCase);
        }
        public bool Equals(PathString other, StringComparison comparisonType)
        {
            if (!HasValue && !other.HasValue)
            {
                return true;
            }
            return string.Equals(_value, other._value, comparisonType);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return !HasValue;
            }
            return obj is PathString && Equals((PathString)obj);
        }
        public override int GetHashCode()
        {
            return (HasValue ? StringComparer.OrdinalIgnoreCase.GetHashCode(_value) : 0);
        }
        public static bool operator ==(PathString left, PathString right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(PathString left, PathString right)
        {
            return !left.Equals(right);
        }
        public static string operator +(string left, PathString right)
        {
            // This overload exists to prevent the implicit string<->PathString converter from
            // trying to call the PathString+PathString operator for things that are not path strings.
            return string.Concat(left, right.ToString());
        }
        public static string operator +(PathString left, string right)
        {
            // This overload exists to prevent the implicit string<->PathString converter from
            // trying to call the PathString+PathString operator for things that are not path strings.
            return string.Concat(left.ToString(), right);
        }
        public static PathString operator +(PathString left, PathString right)
        {
            return left.Add(right);
        }
        public static implicit operator PathString(string s)
        {
            return ConvertFromString(s);
        }
        public static implicit operator string(PathString path)
        {
            return path.ToString();
        }
        internal static PathString ConvertFromString(string s)
        {
            return string.IsNullOrEmpty(s) ? new PathString(s) : FromUriComponent(s);
        }
    }
    #endregion
    public class CallContext
    {
        public PathString Path { get; }
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

    #region Map Util Class Copy From https://github.com/aspnet/HttpAbstractions

    public class MapOptions
    {
        public CallDelegate Branch { get; set; }
        public PathString PathMatch { get; set; }
    }

    public class MapWhenOptions
    {
        private Predicate<CallContext> _predicate;
        public Predicate<CallContext> Predicate
        {
            get
            {
                return _predicate;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _predicate = value;
            }
        }
        public CallDelegate Branch { get; set; }
    }
    public class MapMiddleware
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
            if(path.StartsWithSegments(_options.PathMatch))
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
    public class MapWhenMiddleware
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
            if(context==null)
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

    #endregion
    public class CallBuilder
    {
        private List<Func<CallDelegate, CallDelegate>> middlewareList;
        private static Task CompletedTask = Task.FromResult(false);
        private CallDelegate _last = (context) =>
         {
             return CompletedTask;
         };
        private ILoggerFactory factory = null;
        public CallBuilder(ILoggerFactory loggerFactory)
        {
            factory = loggerFactory;
            middlewareList = new List<Func<CallDelegate, CallDelegate>>();
        }
        private CallBuilder(ILoggerFactory loggerFactory, List<Func<CallDelegate, CallDelegate>> _middlewareList)
        {
            factory = loggerFactory;
            middlewareList = _middlewareList;
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

        public CallBuilder Run(Func<CallContext, Task> last)
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

        #region New
        public CallBuilder New()
        {
            return new CallBuilder(this.factory, this.middlewareList);
        }
        #endregion
        #region Map Copy From https://github.com/aspnet/HttpAbstractions
        public CallBuilder Map(PathString pathMatch, Action<CallBuilder> configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (pathMatch.HasValue && pathMatch.Value.EndsWith("/",StringComparison.Ordinal))
            {
                throw new ArgumentException("The path must not end with a '/'", nameof(pathMatch));
            }
            var branchBuilder = this.New();
            configuration(branchBuilder);
            var branch = branchBuilder.Build();
            var options = new MapOptions
            {
                Branch = branch,
                PathMatch = pathMatch
            };
            return this.Use(next => new MapMiddleware(next, options).Invoke);
        }
        public CallBuilder MapWhen(Predicate<CallContext> predicate, Action<CallBuilder> configuration)
        {
            if(predicate==null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var branchBuilder = this.New();
            configuration(branchBuilder);
            var branch = branchBuilder.Build();
            var options = new MapWhenOptions
            {
                Predicate=predicate,
                Branch=branch
            };
            return this.Use(next => new MapWhenMiddleware(next, options).Invoke);
        }
        #endregion
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