using System;

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

        Type FieldValueOverrideType { get; }

        bool OnlyOverrideFirst { get; }
    }
}