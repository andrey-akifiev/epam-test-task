using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EPAM.StudyGroups.Tests.Integration.Extensions
{
    public static class HttpClientExtensions
    {
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();

        static HttpClientExtensions()
        {
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
        }

        public static async Task<HttpResponseMessage> PostAsync<TRequest>(
            this HttpClient httpClient,
            string endpointUrl,
            TRequest payload,
            string correlationId = null)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpointUrl);
            requestMessage.Headers.Add(CustomHeaderNames.CorrelationId, correlationId ?? Guid.NewGuid().ToString());
            requestMessage.Content = JsonContent.Create(payload);

            return await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PutAsync<TRequest>(
            this HttpClient httpClient,
            string endpointUrl,
            TRequest payload = null,
            string correlationId = null)
                where TRequest : class
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, endpointUrl);
            requestMessage.Headers.Add(CustomHeaderNames.CorrelationId, correlationId ?? Guid.NewGuid().ToString());

            if (payload != null)
            {
                requestMessage.Content = JsonContent.Create(payload);
            }

            return await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        }

        public static async Task<(TResponse Data, HttpResponseMessage Response)> TryGetAsync<TResponse>(
            this HttpClient httpClient,
            string endpointUrl,
            string correlationId = null)
            where TResponse : class
        {
            TResponse data = null;

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, endpointUrl);
            requestMessage.Headers.Add(CustomHeaderNames.CorrelationId, correlationId ?? Guid.NewGuid().ToString());

            var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                data = await response
                    .Content
                    .ReadFromJsonAsync<TResponse>(jsonSerializerOptions)
                    .ConfigureAwait(false);
            }

            return (data, response);
        }

        public static async Task<HttpResponseMessage> TryPostAsync<TRequest>(
            this HttpClient httpClient,
            string endpointUrl,
            TRequest payload,
            string correlationId = null)
        {
            var response = await httpClient.PostAsync(endpointUrl, payload, correlationId).ConfigureAwait(false);

            return response;
        }

        public static async Task<(TResponse Data, HttpResponseMessage Response)> TryPostAsync<TResponse, TRequest>(
            this HttpClient httpClient,
            string endpointUrl,
            TRequest payload,
            string correlationId = null)
            where TResponse : class
        {
            TResponse data = null;

            var response = await httpClient.PostAsync(endpointUrl, payload, correlationId).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadFromJsonAsync<TResponse>().ConfigureAwait(false);
            }

            return (data, response);
        }

        public static async Task<HttpResponseMessage> TryPutAsync<TRequest>(
            this HttpClient httpClient,
            string endpointUrl,
            TRequest payload = null,
            string correlationId = null)
                where TRequest : class
        {
            var response = await httpClient.PutAsync(endpointUrl, payload, correlationId).ConfigureAwait(false);

            return response;
        }
    }
}