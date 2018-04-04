using System;

namespace StandardDot.CoreExtensions.Object.DeepClone
{
    /// <summary>
    /// Settings to override copyable values
    /// </summary>
    public class CopyOverrideSettings<T> : Copyable, ICopyOverrideSettings
    {
        public CopyOverrideSettings(Type containingClassType, string affectedFieldName, T fieldValueOverride, bool onlyOverrideFirst = false)
            : base(containingClassType, affectedFieldName, fieldValueOverride, onlyOverrideFirst)
        {
            ContainingClassType = containingClassType;
            AffectedFieldName = affectedFieldName;
            FieldValueOverride = fieldValueOverride;
            OnlyOverrideFirst = onlyOverrideFirst;
        }

        public Type ContainingClassType { get; }

        public string AffectedFieldName { get; }

        public object FieldValueOverride { get; }

        public Type FieldValueOverrideType => typeof(T);

        public bool OnlyOverrideFirst { get; }
    }
}