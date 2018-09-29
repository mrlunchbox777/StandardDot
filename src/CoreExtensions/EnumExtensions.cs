using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
		public static T? TryParseEnumSafe<T>(this string source)
			where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}

			string trimmedSource = source?.Trim();

			T? target = string.IsNullOrWhiteSpace(trimmedSource)
				? null
				: Enum.TryParse(trimmedSource, true, out T val)
					? val
					: (T?)null;

			return target;
		}

		/// <summary>
		/// Gets an <c>IEnumerable</c> of members in an enum
		/// </summary>
		/// <typeparam name="T">Enum Type</typeparam>
		/// <param name="source">A member of the Enum</param>
		/// <returns>All members of <c>T</c></returns>
		public static IEnumerable<T> GetValues<T>(this T source)
			where T : struct, IConvertible
		{
			return GetValuesOfEnum<T>();
		}

		/// <summary>
		/// Gets an <c>IEnumerable</c> of members in an enum
		/// </summary>
		/// <typeparam name="T">Enum Type</typeparam>
		/// <returns>All members of <c>T</c></returns>
		private static IEnumerable<T> GetValuesOfEnum<T>()
			where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}

			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		/// <summary>
		/// Gets an <c>IEnumerable</c> of members in an enum
		/// </summary>
		/// <param name="source">Enum Type</param>
		/// <returns>All members of <c>T</c></returns>
		public static IEnumerable<Enum> GetValues(this Type enumType)
		{
			if (!enumType.IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}

			var flags = BindingFlags.Static | BindingFlags.NonPublic;
			var getValuesOverloads = typeof(EnumExtensions)
							.GetMember("GetValuesOfEnum", MemberTypes.Method, flags)
							.Cast<MethodInfo>();
			MethodInfo method = getValuesOverloads.Single(x => x.ContainsGenericParameters);
			MethodInfo genericMethod = method.MakeGenericMethod(enumType);

			return genericMethod.Invoke(null, null) as IEnumerable<Enum>;
		}
	}
}