using System;
using JWT;
using StandardDot.CoreServices.Serialization;

namespace StandardDot.Authentication.Jwt
{
	/// <summary>
	/// The serializer that uses the Json Serialization Service
	/// <note>Serializing with this class is slow, it's better not to use it</note>
	/// </summary>
	public class JwtJsonSerializer : Json, IJsonSerializer
	{
		/// <summary>
		/// Deserializes an object using the Standard Dot Json Serializer
		/// </summary>
		/// <param name="json">The json payload to deserialize</param>
		/// <returns>The payload object</returns>
		public T Deserialize<T>(string json) => DeserializeObject<T>(json);

		/// <summary>
		/// Serializes a Json object using the Standard Dot Json Serializer
		/// <note>Serializing with this class is slow, it's better not to use it</note>
		/// </summary>
		/// <param name="obj">The object to Serialize</param>
		/// <returns>The string representation of the string</returns>
		public string Serialize(object obj)
		{
			if (obj == null)
			{
				return "";
			}

			Type objType = obj.GetType();
			var mi = typeof(JwtJsonSerializer).GetMethod("SerializeObject");
			var fooRef = mi.MakeGenericMethod(objType);
			return (string)fooRef.Invoke(this, (dynamic)Convert.ChangeType(obj, objType));
		}
	}
}