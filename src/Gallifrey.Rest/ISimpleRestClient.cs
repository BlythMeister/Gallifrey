using System;
using System.Collections.Generic;
using System.Net;

namespace Gallifrey.Rest
{
    public interface ISimpleRestClient
    {
        void Post(HttpStatusCode expectedStatus, string path, Dictionary<string, object> data = null);
        void Put(HttpStatusCode expectedStatus, string path, Dictionary<string, object> data = null);
        T Get<T>(HttpStatusCode expectedStatus, string path, Dictionary<string, object> data = null, Func<string, T> customDeserialize = null) where T : class;
    }
}