using System;
using System.Runtime.Serialization;

namespace StandardDot.Authentication.Jwt
{
	/// <summary>
	/// The base JwtToken that defines an expiration
	/// </summary>
	public interface IJwtToken
	{
		[DataMember(Name = "exp")]
		DateTime Expiration { get; set; }
	}
}