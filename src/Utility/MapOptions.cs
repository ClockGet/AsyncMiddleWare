using AsyncMiddleWare.Context;
using AsyncMiddleWare.Utility;
using System;

namespace AsyncMiddleWare.Utility
{
    internal class MapOptions
    {
        public CallDelegate Branch { get; set; }
        public PathString PathMatch { get; set; } 
    }
    internal class MapWhenOptions
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
}
