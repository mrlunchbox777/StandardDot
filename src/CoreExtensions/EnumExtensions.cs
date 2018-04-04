using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace StandardDot.CoreExtensions
{
    /// <summary>
    /// Extensions for Enum.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the value of an enum from a string, or returns null
        /// </summary>
        /// <typeparam name="T">Enum Type</typeparam>
        /// <param name="target">Target enum</param>
        /// <param name="source">The string that has the enum</param>
        /// <returns>The enum from the string, or null</returns>
        public static T? TryParseSafe<T>(this T? target, string source)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            target = string.IsNullOrWhiteSpace(source)
                ? null
                : Enum.TryParse(source, out T val)
                    ? val
                    : (T?)null;
                    
            return target;
        }
    }
}