using Gallifrey.Rest.Exception;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Gallifrey.Rest
{
    public class SimpleRestClient : ISimpleRestClient
    {
        private readonly Func<string, List<string>> errorMessageSerializationFunction;
        private readonly RestClient client;
        private readonly string authHeader;
        private readonly string errorResponseDirectory;

        private SimpleRestClient(string baseUrl, string authHeader, Func<string, List<string>> errorMessageSerializationFunction)
        {
            this.authHeader = authHeader;
            this.errorMessageSerializationFunction = errorMessageSerializationFunction;
            errorResponseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gallifrey", "WebErrors");

            if (!Directory.Exists(errorResponseDirectory))
            {
                Directory.CreateDirectory(errorResponseDirectory);
            }

            client = new RestClient(new RestClientOptions(baseUrl) { Timeout = (int)TimeSpan.FromMinutes(2).TotalMilliseconds, AutomaticDecompression = DecompressionMethods.GZip });
        }

        public static SimpleRestClient WithBasicAuthentication(string baseUrl, string username, string password, Func<string, List<string>> errorMessageSerializationFunction)
        {
            var base64BasicAuthText = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            var header = $"Basic {base64BasicAuthText}";
            return new SimpleRestClient(baseUrl, header, errorMessageSerializationFunction);
        }

        public static SimpleRestClient WithBearerAuthentication(string baseUrl, string bearerToken, Func<string, List<string>> errorMessageSerializationFunction)
        {
            var header = $"Bearer {bearerToken}";
            return new SimpleRestClient(baseUrl, header, errorMessageSerializationFunction);
        }

        public void Post(HttpStatusCode expectedStatus, string path, object data = null)
        {
            Execute(Method.Post, expectedStatus, path, data);
        }

        public void Put(HttpStatusCode expectedStatus, string path, object data = null)
        {
            Execute(Method.Put, expectedStatus, path, data);
        }

        public T Get<T>(HttpStatusCode expectedStatus, string path, object data = null, Func<string, T> customDeserialize = null) where T : class
        {
            if (customDeserialize == null)
            {
                customDeserialize = JsonConvert.DeserializeObject<T>;
            }

            var response = Execute(Method.Get, expectedStatus, path, data);

            return customDeserialize(response.Content);
        }

        private RestResponse Execute(Method method, HttpStatusCode expectedStatus, string path, object data = null)
        {
            var requestGuid = Guid.NewGuid();
            var errorSaveDirectory = Path.Combine(errorResponseDirectory, DateTime.Now.ToString("yy-MM-dd"), requestGuid.ToString());
            var request = CreateRequest(method, path, requestGuid);
            if (data != null)
            {
                request.AddHeader("ContentType", "application/json");
                request.AddJsonBody(data);
            }

            RestResponse response;
            try
            {
                response = client.ExecuteAsync(request).Result;
            }
            catch (System.Exception e) when (errorResponseDirectory != null)
            {
                Directory.CreateDirectory(errorSaveDirectory);
                File.WriteAllText(Path.Combine(errorSaveDirectory, "request.txt"), request.AsString(client));
                File.WriteAllText(Path.Combine(errorSaveDirectory, "error.txt"), e.ToString());
                throw;
            }

            try
            {
                AssertStatus(response, expectedStatus);
            }
            catch (System.Exception e)
            {
                Directory.CreateDirectory(errorSaveDirectory);
                File.WriteAllText(Path.Combine(errorSaveDirectory, "request.txt"), request.AsString(client));
                File.WriteAllText(Path.Combine(errorSaveDirectory, "response.txt"), response.AsString());
                File.WriteAllText(Path.Combine(errorSaveDirectory, "error.txt"), e.ToString());
                throw;
            }

            return response;
        }

        private RestRequest CreateRequest(Method method, string path, Guid requestGuid)
        {
            var request = new RestRequest { Method = method, Resource = path, RequestFormat = DataFormat.Json };
            request.AddHeader("Authorization", authHeader);
            request.AddHeader("ContentType", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("TrackingGuid", requestGuid.ToString());
            request.AddHeader("User-Agent", "Gallifrey");
            return request;
        }

        private void AssertStatus(RestResponse response, HttpStatusCode status)
        {
            if (response.ErrorException != null)
            {
                throw new ClientException("Transport level error: " + response.ErrorMessage, response.ErrorException);
            }

            if (response.StatusCode != status)
            {
                try
                {
                    if (errorMessageSerializationFunction != null)
                    {
                        var errorMessages = errorMessageSerializationFunction(response.Content);

                        if (errorMessages.Any())
                        {
                            throw new ClientException($"Wrong status returned: {response.StatusDescription}. Message: {errorMessages.First()}");
                        }
                    }

                    throw new ClientException($"Wrong status returned: {response.StatusDescription}");
                }
                catch (ClientException)
                {
                    throw;
                }
                catch (System.Exception)
                {
                    throw new ClientException($"Wrong status returned: {response.StatusDescription}");
                }
            }
        }
    }
}
