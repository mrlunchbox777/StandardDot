using System;
using JWT;
using StandardDot.Abstract.CoreServices;
using StandardDot.CoreServices.Serialization;

namespace StandardDot.Authentication.Jwt
{
	/// <summary>
	/// The serializer that uses the Json Serialization Service
	/// <note>Serializing with this class is slow, it's better not to use it</note>
	/// </summary>
	public class JwtJsonSerializer : Json, IJsonSerializer
	{
		/// <param name="settings">The settings to use with the Serialization Service</param>
		public JwtJsonSerializer(ISerializationSettings settings = null)
		{
			Settings = settings;
		}

		protected ISerializationSettings Settings { get; }

		/// <summary>
		/// Deserializes an object using the Standard Dot Json Serializer
		/// </summary>
		/// <param name="json">The json payload to deserialize</param>
		/// <returns>The payload object</returns>
		public T Deserialize<T>(string json) => DeserializeObject<T>(json, Settings);

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
			return (string)fooRef.Invoke(this, new object[] { (dynamic)Convert.ChangeType(obj, objType), Settings });
		}
	}
}