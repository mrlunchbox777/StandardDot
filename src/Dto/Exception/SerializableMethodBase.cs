using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace StandardDot.Dto.Exception
{
    /// <summary>
    /// A class that can be used to serialize any MethodBase. Validity of serialization is valued over complete data.
    /// </summary>
    [DataContract]
    public class SerializableMethodBase
    {
        /// <param name="methodBase">The methodbase to convert to serializable form</param>
        public SerializableMethodBase(MethodBase methodBase)
        {
            if (methodBase == null)
            {
                return;
            }

            IsFamily = methodBase.IsFamily;
            IsFamilyAndAssembly = methodBase.IsFamilyAndAssembly;
            IsFamilyOrAssembly = methodBase.IsFamilyOrAssembly;
            IsFinal = methodBase.IsFinal;
            IsGenericMethod = methodBase.IsGenericMethod;
            IsGenericMethodDefinition = methodBase.IsGenericMethodDefinition;
            IsHideBySig = methodBase.IsHideBySig;
            IsPrivate = methodBase.IsPrivate;
            IsPublic = methodBase.IsPublic;
            IsSecurityCritical = methodBase.IsSecurityCritical;
            IsSecuritySafeCritical = methodBase.IsSecuritySafeCritical;
            IsSecurityTransparent = methodBase.IsSecurityTransparent;
            IsSpecialName = methodBase.IsSpecialName;
            IsStatic = methodBase.IsStatic;
            IsVirtual = methodBase.IsVirtual;
            IsConstructor = methodBase.IsConstructor;
            if (methodBase.MethodHandle != null)
            {
                MethodHandle = new SerializableRuntimeMethodHandle(methodBase.MethodHandle);
            }
            IsAssembly = methodBase.IsAssembly;
            ContainsGenericParameters = methodBase.ContainsGenericParameters;
            IsAbstract = IsAbstract = methodBase.IsAbstract;
            MethodImplementationFlags = (SerializableMethodImplAttributes)((int)methodBase.MethodImplementationFlags);
            CallingConvention = (SerialiableCallingConventions)((int)methodBase.CallingConvention);
            Attributes = (SerializableMethodAttributes)((int)methodBase.Attributes);
        }

        [DataMember(Name = "isFamily")]
        public virtual bool IsFamily { get; set; }

        [DataMember(Name = "isFamilyAndAssembly")]
        public virtual bool IsFamilyAndAssembly { get; set; }

        [DataMember(Name = "isFamilyOrAssembly")]
        public virtual bool IsFamilyOrAssembly { get; set; }

        [DataMember(Name = "isFinal")]
        public virtual bool IsFinal { get; set; }

        [DataMember(Name = "isGenericMethod")]
        public virtual bool IsGenericMethod { get; set; }

        [DataMember(Name = "isGenericMethodDefinition")]
        public virtual bool IsGenericMethodDefinition { get; set; }

        [DataMember(Name = "isHideBySig")]
        public virtual bool IsHideBySig { get; set; }

        [DataMember(Name = "isPrivate")]
        public virtual bool IsPrivate { get; set; }

        [DataMember(Name = "isPublic")]
        public virtual bool IsPublic { get; set; }

        [DataMember(Name = "isSecurityCritical")]
        public virtual bool IsSecurityCritical { get; set; }

        [DataMember(Name = "isSecuritySafeCritical")]
        public virtual bool IsSecuritySafeCritical { get; set; }

        [DataMember(Name = "isSecurityTransparent")]
        public virtual bool IsSecurityTransparent { get; set; }

        [DataMember(Name = "isSpecialName")]
        public virtual bool IsSpecialName { get; set; }

        [DataMember(Name = "isStatic")]
        public virtual bool IsStatic { get; set; }

        [DataMember(Name = "isVirtual")]
        public virtual bool IsVirtual { get; set; }

        [DataMember(Name = "isConstructor")]
        public virtual bool IsConstructor { get; set; }

        [DataMember(Name = "rethodHandle")]
        public virtual SerializableRuntimeMethodHandle MethodHandle { get; set; }

        [DataMember(Name = "isAssembly")]
        public virtual bool IsAssembly { get; set; }

        [DataMember(Name = "containsGenericParameters")]
        public virtual bool ContainsGenericParameters { get; set; }

        [DataMember(Name = "isAbstract")]
        public virtual bool IsAbstract { get; set; }

        [DataMember(Name = "methodImplementationFlags")]
        public virtual SerializableMethodImplAttributes MethodImplementationFlags { get; set; }

        [DataMember(Name = "callingConvention")]
        public virtual SerialiableCallingConventions CallingConvention { get; set; }

        [DataMember(Name = "attributes")]
        public virtual SerializableMethodAttributes Attributes { get; set; }
    }
}