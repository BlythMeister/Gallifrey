using Newtonsoft.Json;
using RestSharp;
using System.Linq;

namespace Gallifrey.Rest
{
    public static class RestClientExtensions
    {
        public static string AsString(this IRestRequest request, RestClient client)
        {
            var requestToLog = new
            {
                resource = request.Resource,
                parameters = request.Parameters.Select(p => new
                {
                    name = p.Name,
                    value = p.Value,
                    type = p.Type.ToString()
                }),
                method = request.Method.ToString(),
                uri = client.BuildUri(request)
            };

            return JsonConvert.SerializeObject(requestToLog, Formatting.Indented);
        }

        public static string AsString(this IRestResponse response)
        {
            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage
            };

            return JsonConvert.SerializeObject(responseToLog, Formatting.Indented);
        }
    }
}
