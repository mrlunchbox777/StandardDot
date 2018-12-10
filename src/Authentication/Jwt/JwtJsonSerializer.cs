// using System;
// using JWT;
// using StandardDot.CoreServices.Serialization;

// namespace StandardDot.Authentication.Jwt
// {
// 	/// <summary>
// 	/// The serializer that uses the Json Serialization Service
// 	/// <note>Serializing with this class is slow, it's better not to use it</note>
// 	/// </summary>
// 	public class JwtJsonSerializer : Json, IJsonSerializer
// 	{
// 		public T Deserialize<T>(string json) => DeserializeObject<T>(json);

// 		public string Serialize(object obj)
// 		{
// 			if (obj == null)
// 			{
// 				return "";
// 			}

// 			Type objType = obj.GetType();
// 			var mi = typeof(JwtJsonSerializer).GetMethod("SerializeObject");
// 			var fooRef = mi.MakeGenericMethod(objType);
// 			return (string)fooRef.Invoke(this, (dynamic)Convert.ChangeType(obj, objType));
// 		}
// 	}
// }