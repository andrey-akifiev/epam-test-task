using System.Net.Http.Json;

namespace EPAM.StudyGroups.Tests.Integration.Extensions
{
    public static class HttpClientExtensions
    {
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
    }
}