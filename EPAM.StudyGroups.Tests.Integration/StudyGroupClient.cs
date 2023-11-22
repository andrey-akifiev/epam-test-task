using EPAM.StudyGroups.Api.Models;
using EPAM.StudyGroups.Data.Models;
using EPAM.StudyGroups.Tests.Integration.Extensions;
using FluentAssertions;

namespace EPAM.StudyGroups.Tests.Integration
{
    public class StudyGroupClient : IDisposable
    {
        private readonly HttpClient httpClient;

        public StudyGroupClient(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task CreateStudyGroupAsync(CreateStudyGroupRequest request, string correlationId = null)
        {
            return this.TryCreateStudyGroupAsync(request, correlationId);
        }

        public Task<HttpResponseMessage> TryCreateStudyGroupAsync(
            CreateStudyGroupRequest request,
            string correlationId = null)
        {
            return this.httpClient.TryPostAsync("/studygroup/create", request, correlationId);
        }

        public Task<StudyGroup[]> GetStudyGroupsAsync(string correlationId = null)
        {
            return FromTryMethodAsync<StudyGroup[]>(new(() => this.TryGetStudyGroupsAsync(correlationId)));
        }

        public Task<(StudyGroup[] data, HttpResponseMessage response)> TryGetStudyGroupsAsync(string correlationId = null)
        {
            return this.httpClient.TryGetAsync<StudyGroup[]>("/studygroup", correlationId);
        }

        public Task<StudyGroup[]> SearchStudyGroupsAsync(
            string subject,
            string correlationId = null)
        {
            return FromTryMethodAsync<StudyGroup[]>(new(() => this.TrySearchStudyGroupsAsync(correlationId)));
        }

        public Task<(StudyGroup[] data, HttpResponseMessage response)> TrySearchStudyGroupsAsync(
            string subject,
            string correlationId = null)
        {
            return this.httpClient.TryGetAsync<StudyGroup[]>($"/studygroup/{subject}", correlationId);
        }

        public void Dispose()
        {
            this.httpClient?.Dispose();
        }

        private async Task<TResponse> FromTryMethodAsync<TResponse>(Func<Task<(TResponse, HttpResponseMessage)>> func)
        {
            (var data, var response) = await func().ConfigureAwait(false);

            data.Should().NotBeNull(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            return data;
        }
    }
}