using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using StandardDot.CoreExtensions;
using StandardDot.CoreExtensions.Object;
using StandardDot.Dto.Exception;
using Xunit;

namespace StandardDot.Dto.IntegrationTests.Exception
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

		[Fact]
		public void ExceptionPrimitiveProperties()
		{
			SerializableException sException;
			System.Exception exception;
			string message = "test";

			try
			{
				throw new CustomException(4, "a stack trace", message, null, null);
			}
			catch (CustomException ex)
			{
				exception = ex;
				sException = new SerializableException(exception, false, false);
			}

			AssertEqualExceptions(sException, exception, false, false);
		}

		[Fact]
		public void ExceptionPrimativeDictionary()
		{
			SerializableException sException;
			System.Exception exception;
			string message = "test";

			Dictionary<string, object> data = new Dictionary<string, object>
			{
				{ "test", 16 },
				{ "foo", "bar" }
			};

			try
			{
				throw new CustomException(4, "a stack trace", message, data, null);
			}
			catch (CustomException ex)
			{
				exception = ex;
				sException = new SerializableException(exception, false, true);
			}

			Assert.Equal((object)"bar", sException.Data["foo"]);
			Assert.Equal(16, (int)(sException.Data["test"]));
			AssertEqualExceptions(sException, exception, false, true);
		}

		[Fact]
		public void ExceptionObjectDictionary()
		{
			SerializableException sException;
			System.Exception exception;
			string message = "test";

			Guid guid = Guid.NewGuid();
			SerializableException serializableException = new SerializableException(new InvalidOperationException(message));
			Dictionary<object, object> data = new Dictionary<object, object>
			{
				{ guid, 16 },
				{ "foo", serializableException }
			};

			try
			{
				throw new CustomException(4, "a stack trace", message, data, null);
			}
			catch (CustomException ex)
			{
				exception = ex;
				sException = new SerializableException(exception, false, true);
			}

			Assert.Equal(16, (int)(sException.Data[guid]));
			Assert.Equal((object)serializableException, sException.Data["foo"]);
			AssertEqualExceptions(sException, exception, false, true);
		}

		[Fact]
		public void ExceptionMixedObjectDictionary()
		{
			SerializableException sException;
			System.Exception exception;
			string message = "test";

			Guid guid = Guid.NewGuid();
			SerializableException serializableException = new SerializableException(new InvalidOperationException(message));
			Dictionary<object, object> data = new Dictionary<object, object>
			{
				{ guid, 16 },
				{ "test", 16 },
				{ "foo", serializableException }
			};

			try
			{
				throw new CustomException(4, "a stack trace", message, data, null);
			}
			catch (CustomException ex)
			{
				exception = ex;
				sException = new SerializableException(exception, false, true);
			}

			Assert.Equal(16, (int)(sException.Data[guid]));
			Assert.Equal((object)serializableException, sException.Data["foo"]);
			Assert.Equal(16, (int)(sException.Data["test"]));
			AssertEqualExceptions(sException, exception, false, true);
		}

		[Fact]
		public void ExceptionNonSerializableDictionary()
		{
			SerializableException sException;
			System.Exception exception;
			string message = "test";

			Guid guid = Guid.NewGuid();
			Stream memoryStream = new MemoryStream();
			Dictionary<object, object> data = new Dictionary<object, object>
			{
				{ guid, 16 },
				{ "test", 16 },
				{ "foo", memoryStream }
			};

			try
			{
				throw new CustomException(4, "a stack trace", message, data, null);
			}
			catch (CustomException ex)
			{
				exception = ex;
				sException = new SerializableException(exception, false, true);
			}

			Assert.Equal(16, (int)(sException.Data[guid]));
			Assert.Null(sException.Data["foo"]);
			Assert.Equal(16, (int)(sException.Data["test"]));
			AssertEqualExceptions(sException, exception, false, true);
			memoryStream.Dispose();
		}

		[Fact]
		public void ExceptionFullyMixedDictionary()
		{
			SerializableException sException;
			System.Exception exception;
			string message = "test";
			string innerMessage = "inner test";

			Guid guid = Guid.NewGuid();
			Stream memoryStream = new MemoryStream();
			SerializableException serializableException = new SerializableException(new InvalidOperationException(message));
			Dictionary<object, object> data = new Dictionary<object, object>
			{
				{ guid, 16 },
				{ "test", serializableException },
				{ "foo", memoryStream }
			};

			try
			{
				throw new CustomException(4, "a stack trace", message, data, new CustomException(4, "a stack trace", innerMessage, null, null));
			}
			catch (CustomException ex)
			{
				exception = ex;
				sException = new SerializableException(exception, true, true);
			}

			Assert.Equal(16, (int)(sException.Data[guid]));
			Assert.Null(sException.Data["foo"]);
			Assert.Equal((object)serializableException, sException.Data["test"]);
			AssertEqualExceptions(sException, exception, true, true);
			memoryStream.Dispose();
		}

		[Fact]
		public void ExceptionFullyMixedDictionarySerialization()
		{
			SerializableException sException;
			System.Exception exception;
			string message = "test";
			string innerMessage = "inner test";

			Guid guid = Guid.NewGuid();
			Stream memoryStream = new MemoryStream();
			SerializableException serializableException = new SerializableException(new InvalidOperationException(message));
			Dictionary<object, object> data = new Dictionary<object, object>
			{
				{ guid, 16 },
				{ "test", serializableException },
				{ "foo", memoryStream }
			};

			try
			{
				throw new CustomException(4, "a stack trace", message, data, new CustomException(4, "a stack trace", innerMessage, null, null));
			}
			catch (CustomException ex)
			{
				exception = ex;
				sException = new SerializableException(exception, true, true);
			}

			Assert.Equal(16, (int)(sException.Data[guid]));
			Assert.Null(sException.Data["foo"]);
			Assert.Equal((object)serializableException, sException.Data["test"]);
			AssertEqualExceptions(sException, exception, true, true);

			string jsonString = sException.SerializeJson();
			Assert.NotNull(jsonString);
			Assert.NotEmpty(jsonString);
			SerializableException deserialized = jsonString.DeserializeJson<SerializableException>();
			Assert.NotNull(deserialized);
			// the data can't be verified this way (they are new instances when deserialized)
			AssertEqualExceptionsSerializable(sException, deserialized, true, false);
			Assert.Equal(sException.Data.Count, deserialized.Data.Count);
			string guidKeyString = deserialized.Data.Keys.OfType<string>().FirstOrDefault(k => k == guid.ToString());
			Assert.NotNull(guidKeyString);
			Assert.NotEmpty(guidKeyString);
			Guid guidKey = Guid.Parse(guidKeyString);
			Assert.NotNull(deserialized.Data[guidKeyString]);
			Assert.Equal(sException.Data[guid], deserialized.Data[guidKeyString]);
			AssertEqualExceptionsSerializable(serializableException, (SerializableException)deserialized.Data["test"], true, true);
			Assert.Null(deserialized.Data["foo"]);

			memoryStream.Dispose();
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
				return;
			}

			if (!(data.Count > 0))
			{
				Assert.Empty(sData);
				return;
			}

			foreach (DictionaryEntry item in data)
			{
				if (item.Key == null)
				{
					Assert.False(sData.Contains(item.Key));
					continue;
				}
				Type keyType = item.Key.GetType();
				Type valueType = item.Value?.GetType();
				if (!(keyType.IsValueType || typeof(string).IsAssignableFrom(keyType) || typeof(decimal).IsAssignableFrom(keyType)
					|| keyType.CustomAttributes.FirstOrDefault(a => typeof(DataContractAttribute).IsAssignableFrom(a.AttributeType)) != null))
				{
					Assert.False(sData.Contains(item.Key));
					continue;
				}
				if (valueType == null || !(valueType.IsValueType || typeof(string).IsAssignableFrom(valueType) || typeof(decimal).IsAssignableFrom(valueType)
					|| valueType.CustomAttributes.FirstOrDefault(a => typeof(DataContractAttribute).IsAssignableFrom(a.AttributeType)) != null))
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

		private static void AssertEqualExceptionsSerializable(SerializableException sException, SerializableException exception, bool checkTargetSite, bool checkData)
		{
			Assert.Equal(exception.HelpLink, sException.HelpLink);
			Assert.Equal(exception.HResult, sException.HResult);
			Assert.Equal(exception.Message, sException.Message);
			Assert.Equal(exception.Source, sException.Source);
			Assert.Equal(exception.StackTrace, sException.StackTrace);

			if (checkTargetSite)
			{
				AssertEqualTargetSitesSerializable(sException.TargetSite, exception.TargetSite);
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
			AssertEqualExceptionsSerializable(sException.InnerException, exception.InnerException, checkTargetSite, checkData);
		}

		private static void AssertEqualTargetSitesSerializable(SerializableMethodBase sTargetSite, SerializableMethodBase targetSite)
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
			AssertEqualMethodHandleSerializable(sTargetSite.MethodHandle, targetSite.MethodHandle);
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

		private static void AssertEqualMethodHandleSerializable(SerializableRuntimeMethodHandle sMethodHandle, SerializableRuntimeMethodHandle methodHandle)
		{
			if (methodHandle.Equals(default(SerializableRuntimeMethodHandle)))
			{
				Assert.Equal(default(SerializableRuntimeMethodHandle), sMethodHandle);
			}

			Assert.Equal(methodHandle.Value, sMethodHandle.Value);
		}
	}
}
