using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using StandardDot.Dto.Exception;
using Xunit;

namespace StandardDot.Dto.UnitTests
{
    public class SerializableExceptionTests
    {
        [Fact]
        public void ExceptionConversion()
        {
            SerializableException sException;
            System.Exception exception;
            string message = "test";
            string innerMessage = "inner test";

            try
            {
                throw new InvalidOperationException(message, new InvalidCastException(innerMessage));
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
                sException = new SerializableException(exception, true, false);
            }

            AssertEqualExceptions(sException, exception, true, false);
        }

        private static void AssertEqualExceptions(SerializableException sException, System.Exception exception, bool checkTargetSite, bool checkData)
        {
            Assert.Equal(exception.HelpLink, sException.HelpLink);
            Assert.Equal(exception.HResult, sException.HResult);
            Assert.Equal(exception.Message, sException.Message);
            Assert.Equal(exception.Source, sException.Source);
            Assert.Equal(exception.StackTrace, sException.StackTrace);

            if (checkTargetSite)
            {
                AssertEqualTargetSites(sException.TargetSite, exception.TargetSite);
            }
            if (checkData)
            {
                AssertEqualData(sException.Data, exception.Data);
            }

            if (exception.InnerException == null)
            {
                Assert.Null(sException.InnerException);
                return;
            }
            AssertEqualExceptions(sException.InnerException, exception.InnerException, checkTargetSite, checkData);
        }

        private static void AssertEqualTargetSites(SerializableMethodBase sTargetSite, MethodBase targetSite)
        {
            if (targetSite == null)
            {
                Assert.Null(targetSite);
                return;
            }
            
            Assert.Equal(targetSite.IsFamily, sTargetSite.IsFamily);
            Assert.Equal(targetSite.IsFamilyAndAssembly, sTargetSite.IsFamilyAndAssembly);
            Assert.Equal(targetSite.IsFamilyOrAssembly, sTargetSite.IsFamilyOrAssembly);
            Assert.Equal(targetSite.IsFinal, sTargetSite.IsFinal);
            Assert.Equal(targetSite.IsGenericMethod, sTargetSite.IsGenericMethod);
            Assert.Equal(targetSite.IsGenericMethodDefinition, sTargetSite.IsGenericMethodDefinition);
            Assert.Equal(targetSite.IsHideBySig, sTargetSite.IsHideBySig);
            Assert.Equal(targetSite.IsPrivate, sTargetSite.IsPrivate);
            Assert.Equal(targetSite.IsPublic, sTargetSite.IsPublic);
            Assert.Equal(targetSite.IsSecurityCritical, sTargetSite.IsSecurityCritical);
            Assert.Equal(targetSite.IsSecuritySafeCritical, sTargetSite.IsSecuritySafeCritical);
            Assert.Equal(targetSite.IsSecurityTransparent, sTargetSite.IsSecurityTransparent);
            Assert.Equal(targetSite.IsSpecialName, sTargetSite.IsSpecialName);
            Assert.Equal(targetSite.IsVirtual, sTargetSite.IsVirtual);
            Assert.Equal(targetSite.IsConstructor, sTargetSite.IsConstructor);
            AssertEqualMethodHandle(sTargetSite.MethodHandle, targetSite.MethodHandle);
            Assert.Equal(targetSite.IsAssembly, sTargetSite.IsAssembly);
            Assert.Equal(targetSite.ContainsGenericParameters, sTargetSite.ContainsGenericParameters);
            Assert.Equal(targetSite.IsAbstract, sTargetSite.IsAbstract);
            Assert.Equal((int)targetSite.MethodImplementationFlags, (int)sTargetSite.MethodImplementationFlags);
            Assert.Equal((int)targetSite.CallingConvention, (int)sTargetSite.CallingConvention);
            Assert.Equal((int)targetSite.Attributes, (int)sTargetSite.Attributes);
            // Because of duplicated enum values the string outputs won't always be the same
            //Assert.Equal(targetSite.MethodImplementationFlags.ToString(), sTargetSite.MethodImplementationFlags.ToString());
            Assert.Equal(targetSite.CallingConvention.ToString(), sTargetSite.CallingConvention.ToString());
            // Because of duplicated enum values the string outputs won't always be the same
            //Assert.Equal(targetSite.Attributes.ToString(), sTargetSite.Attributes.ToString());
        }

        private static void AssertEqualData(IDictionary sData, IDictionary data)
        {
            if (data == null)
            {
                Assert.Null(sData);
            }

            if (!(data.Count > 0))
            {
                Assert.Empty(sData);
            }
            
            foreach(DictionaryEntry item in data)
            {
                if (item.Key == null)
                {
                    Assert.False(sData.Contains(item.Key));
                    continue;
                }
                Type keyType = item.Key.GetType();
                Type valueType = item.Value?.GetType();
                if (!(keyType.IsValueType 
                    || keyType.CustomAttributes.FirstOrDefault(a => typeof(DataContractAttribute).IsAssignableFrom(a.GetType())) != null))
                {
                    Assert.False(sData.Contains(item.Key));
                    continue;
                }
                if (!(valueType == null || valueType.IsValueType 
                    || valueType.CustomAttributes.FirstOrDefault(a => typeof(DataContractAttribute).IsAssignableFrom(a.GetType())) != null))
                {
                    Assert.False(sData.Contains(item.Key));
                    continue;
                }
                Assert.True(sData.Contains(item.Key));
                Assert.Equal(item.Value, sData[item.Key]);
            }
        }

        private static void AssertEqualMethodHandle(SerializableRuntimeMethodHandle sMethodHandle, RuntimeMethodHandle methodHandle)
        {
            if (methodHandle == default(RuntimeMethodHandle))
            {
                Assert.Equal(default(SerializableRuntimeMethodHandle), sMethodHandle);
            }
            
            Assert.Equal(methodHandle.Value, sMethodHandle.Value);
        }
    }
}
