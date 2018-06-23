using System;
using System.Net;

namespace Gallifrey.Rest
{
    public interface ISimpleRestClient
    {
        void Post(HttpStatusCode expectedStatus, string path, object data = null);
        void Put(HttpStatusCode expectedStatus, string path, object data = null);
        T Get<T>(HttpStatusCode expectedStatus, string path, object data = null, Func<string, T> customDeserialize = null) where T : class;
    }
}