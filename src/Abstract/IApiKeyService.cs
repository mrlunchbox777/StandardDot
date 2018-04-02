using System.Collections.Generic;

namespace shoellibraries.Abstract
{
    public interface IApiKeyService : IDictionary<string, string>
    {
        // this should return secret keys based on appIds
    }
}