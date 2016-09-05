using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Gallifrey.Rest.Exception;
using Newtonsoft.Json;
using RestSharp;

namespace Gallifrey.Rest
{
    public class SimpleRestClient : ISimpleRestClient
    {
        private readonly Func<string, List<string>> errorMessageSerializationFunction;
        private readonly RestClient client;
        private readonly string base64BasicAuthText;

        public SimpleRestClient(string baseUrl, string username, string password, Func<string, List<string>> errorMessageSerializationFunction)
        {
            this.errorMessageSerializationFunction = errorMessageSerializationFunction;
            base64BasicAuthText = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            client = new RestClient { BaseUrl = new Uri(baseUrl), Timeout = (int)TimeSpan.FromMinutes(2).TotalMilliseconds };
        }

        public void Post(HttpStatusCode expectedStatus, string path, Dictionary<string, object> data = null)
        {
            Execute(Method.POST, expectedStatus, path, data);
        }

        public void Put(HttpStatusCode expectedStatus, string path, Dictionary<string, object> data = null)
        {
            Execute(Method.PUT, expectedStatus, path, data);
        }

        public T Get<T>(HttpStatusCode expectedStatus, string path, Dictionary<string, object> data = null, Func<string, T> customDeserialize = null) where T : class
        {
            if (customDeserialize == null)
            {
                customDeserialize = JsonConvert.DeserializeObject<T>;
            }

            var response = Execute(Method.GET, expectedStatus, path, data);

            return customDeserialize(response.Content);
        }

        private IRestResponse Execute(Method method, HttpStatusCode expectedStatus, string path, Dictionary<string, object> data = null)
        {
            var request = CreateRequest(method, path);
            if (data != null)
            {
                request.AddHeader("ContentType", "application/json");
                request.AddBody(data);
            }

            var response = client.Execute(request);

            AssertStatus(response, expectedStatus);

            return response;
        }

        private RestRequest CreateRequest(Method method, string path)
        {
            var request = new RestRequest { Method = method, Resource = path, RequestFormat = DataFormat.Json };
            request.AddHeader("Authorization", "Basic " + base64BasicAuthText);
            request.AddHeader("ContentType", "application/json");
            request.AddHeader("Accept", "application/json");
            return request;
        }

        private void AssertStatus(IRestResponse response, HttpStatusCode status)
        {
            if (response.ErrorException != null)
                throw new ClientException("Transport level error: " + response.ErrorMessage, response.ErrorException);
            if (response.StatusCode != status)
            {
                try
                {
                    var errorMessages = errorMessageSerializationFunction(response.Content);

                    if (errorMessages.Any())
                    {
                        throw new ClientException($"Wrong status returned: {response.StatusDescription}. Message: {errorMessages.First()}");
                    }
                    else
                    {
                        throw new ClientException($"Wrong status returned: {response.StatusDescription}");
                    }
                }
                catch (ClientException)
                {
                    throw;
                }
                catch (System.Exception ex)
                {
                    throw new ClientException($"Wrong status returned: {response.StatusDescription}", ex);
                }
            }
        }
    }
}
