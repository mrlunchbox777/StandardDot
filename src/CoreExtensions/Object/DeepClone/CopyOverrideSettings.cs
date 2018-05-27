using System;
using System.Collections.Generic;

namespace StandardDot.CoreExtensions.Object.DeepClone
{
    /// <summary>
    /// Settings to override copyable values
    /// </summary>
    public class CopyOverrideSettings<T> : Copyable, ICopyOverrideSettings
    {
        public CopyOverrideSettings(Type containingClassType, string affectedFieldName, T fieldValueOverride, bool shouldSkipOverrideInsteadOfSet, Type defaultPostActionType, bool useVistedGraph,
            bool onlyOverrideFirst = false, bool includeNonPublic = true, Dictionary<Type, Action<dynamic, dynamic, Type>> postCopyActions = null, List<string> fullNamesToSkip = null, List<string> fullNamesToInclude = null,
            List<string> fullNamesToAttempt = null)
        {
            ContainingClassType = containingClassType;
            AffectedFieldName = affectedFieldName;
            FieldValueOverride = fieldValueOverride;
            UseFieldValueOverrideFunction = false;
            OnlyOverrideFirst = onlyOverrideFirst;
            PostCopyActions = postCopyActions;
            FullNamesToSkip = fullNamesToSkip;
            DefaultPostActionType = defaultPostActionType;
            FullNamesToInclude = fullNamesToInclude;
            FullNamesToAttempt = fullNamesToAttempt;
            UseVisitedGraph = useVistedGraph;
            ShouldSkipOverrideInsteadOfSet = shouldSkipOverrideInsteadOfSet;
            IncludeNonPublic = includeNonPublic;
        }
        public CopyOverrideSettings(Type containingClassType, string affectedFieldName, Func<object, object> fieldValueOverrideFunction, bool shouldSkipOverrideInsteadOfSet, Type defaultPostActionType, bool useVistedGraph,
            bool onlyOverrideFirst = false, bool includeNonPublic = true, Dictionary<Type, Action<dynamic, dynamic, Type>> postCopyActions = null, List<string> fullNamesToSkip = null, List<string> fullNamesToInclude = null,
            List<string> fullNamesToAttempt = null)
        {
            ContainingClassType = containingClassType;
            AffectedFieldName = affectedFieldName;
            FieldValueOverrideFunction = fieldValueOverrideFunction;
            UseFieldValueOverrideFunction = true;
            OnlyOverrideFirst = onlyOverrideFirst;
            PostCopyActions = postCopyActions;
            FullNamesToSkip = fullNamesToSkip;
            DefaultPostActionType = defaultPostActionType;
            FullNamesToInclude = fullNamesToInclude;
            FullNamesToAttempt = fullNamesToAttempt;
            UseVisitedGraph = useVistedGraph;
            ShouldSkipOverrideInsteadOfSet = shouldSkipOverrideInsteadOfSet;
            IncludeNonPublic = includeNonPublic;
        }

        public Type ContainingClassType { get; }

        public string AffectedFieldName { get; }

        public object FieldValueOverride { get; }

        public Func<object, object> FieldValueOverrideFunction { get; }

        public bool UseFieldValueOverrideFunction { get; }

        public bool ShouldSkipOverrideInsteadOfSet { get; }

        public Type FieldValueOverrideType => typeof(T);

        public bool IncludeNonPublic { get; }

        public bool OnlyOverrideFirst { get; }

        public Type DefaultPostActionType { get; }

        public bool UseVisitedGraph { get; }

        public Dictionary<Type, Action<dynamic, dynamic, Type>> PostCopyActions { get; }
        
        public List<string> FullNamesToSkip { get; }

        public List<string> FullNamesToInclude { get; }

        public List<string> FullNamesToAttempt { get; }
    }
}