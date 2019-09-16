using System;
using System.Collections.Generic;

namespace StandardDot.CoreExtensions.Object.DeepClone
{
	/// <summary>
	/// Settings to override copyable values
	/// </summary>
	public interface ICopyOverrideSettings
	{
		Type ContainingClassType { get; }

		string AffectedFieldName { get; }

		object FieldValueOverride { get; }

		Func<object, object> FieldValueOverrideFunction { get; }

		bool UseFieldValueOverrideFunction { get; }

		bool ShouldSkipOverrideInsteadOfSet { get; }

		Type FieldValueOverrideType { get; }

		bool OnlyOverrideFirst { get; }

		bool UseVisitedGraph { get; }

		bool IncludeNonPublic { get; }

		Dictionary<Type, Action<dynamic, dynamic, Type>> PostCopyActions { get; }

		Type DefaultPostActionType { get; }

		List<string> FullNamesToSkip { get; }

		List<string> FullNamesToInclude { get; }

		List<string> FullNamesToAttempt { get; }
	}
}