using EPAM.StudyGroups.Api.Models;
using EPAM.StudyGroups.Data.Models;
using EPAM.StudyGroups.Tests.Integration.Extensions;
using EPAM.StudyGroups.Tests.Integration.Models;
using FluentAssertions;

namespace EPAM.StudyGroups.Tests.Integration
{
    public class StudyGroupClient : IDisposable
    {
        private readonly HttpClient httpClient;

        protected StudyGroupClient()
        {
        }

        public StudyGroupClient(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public virtual HttpClient Http => this.httpClient;

        public virtual Task CreateStudyGroupAsync(CreateStudyGroupRequest request, string correlationId = null)
        {
            return this.TryCreateStudyGroupAsync(request, correlationId);
        }

        public virtual Task<HttpResponseMessage> TryCreateStudyGroupAsync(
            CreateStudyGroupRequest request,
            string correlationId = null)
        {
            return this.Http.TryPostAsync("/studygroup/create", request, correlationId);
        }

        public virtual Task<TestStudyGroup[]> GetStudyGroupsAsync(string correlationId = null)
        {
            return FromTryMethodAsync<TestStudyGroup[]>(new(() => this.TryGetStudyGroupsAsync(correlationId)));
        }

        public virtual Task<(TestStudyGroup[] data, HttpResponseMessage response)> TryGetStudyGroupsAsync(string correlationId = null)
        {
            return this.Http.TryGetAsync<TestStudyGroup[]>("/studygroup", correlationId);
        }

        public virtual Task<StudyGroup[]> SearchStudyGroupsAsync(
            string subject,
            string correlationId = null)
        {
            return FromTryMethodAsync<StudyGroup[]>(new(() => this.TrySearchStudyGroupsAsync(subject, correlationId)));
        }

        public virtual Task<(StudyGroup[] data, HttpResponseMessage response)> TrySearchStudyGroupsAsync(
            string subject,
            string correlationId = null)
        {
            return this.Http.TryGetAsync<StudyGroup[]>($"/studygroup/search?{nameof(subject)}={subject}", correlationId);
        }

        public virtual Task JoinStudyGroupAsync(
            int studyGroupId,
            int userId,
            string correlationId = null)
        {
            return this.TryJoinStudyGroupAsync(studyGroupId.ToString(), userId.ToString(), correlationId);
        }

        public virtual Task<HttpResponseMessage> TryJoinStudyGroupAsync(
            string studyGroupId,
            string userId,
            string correlationId = null)
        {
            return this.Http.TryPutAsync<object>(
                $"/studygroup/join?{nameof(studyGroupId)}={studyGroupId}&{nameof(userId)}={userId}", correlationId: correlationId);
        }

        public virtual Task LeaveStudyGroupAsync(
            int studyGroupId,
            int userId,
            string correlationId = null)
        {
            return this.TryLeaveStudyGroupAsync(studyGroupId.ToString(), userId.ToString(), correlationId);
        }

        public virtual Task<HttpResponseMessage> TryLeaveStudyGroupAsync(
            string studyGroupId,
            string userId,
            string correlationId = null)
        {
            return this.Http.TryPutAsync<object>(
                $"/studygroup/leave?{nameof(studyGroupId)}={studyGroupId}&{nameof(userId)}={userId}", correlationId);
        }

        public virtual void Dispose()
        {
            this.Http?.Dispose();
        }

        private async Task<TResponse> FromTryMethodAsync<TResponse>(Func<Task<(TResponse, HttpResponseMessage)>> func)
        {
            (var data, var response) = await func().ConfigureAwait(false);

            response.IsSuccessStatusCode.Should().BeTrue(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            data.Should().NotBeNull(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            return data;
        }
    }
}