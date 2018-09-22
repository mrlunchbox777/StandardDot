using System.Collections.Generic;

namespace StandardDot.Abstract
{
	/// <summary>
	/// An interface for managing API Keys (appId, apiKey)
	/// </summary>
	public interface IApiKeyService : IDictionary<string, string>
	{
	}
}