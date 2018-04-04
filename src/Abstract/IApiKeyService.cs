using System.Collections.Generic;

namespace StandardDot.Abstract
{
    public interface IApiKeyService : IDictionary<string, string>
    {
        // this should return secret keys based on appIds
    }
}