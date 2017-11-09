using System;
using System.Text;

namespace AsyncMiddleWare.Utility
{
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
}
