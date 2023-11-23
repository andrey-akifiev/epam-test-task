using EPAM.StudyGroups.Api.Models;
using EPAM.StudyGroups.Data;
using EPAM.StudyGroups.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPAM.StudyGroups.Tests.Integration
{
    public class StudyGroupPersistenceClient : StudyGroupClient, IDisposable
    {
        private readonly StudyGroupClient wrappedClient;

        private readonly string dbConnectionString;

        private readonly List<StudyGroup> createdStudyGroups = new List<StudyGroup>();

        public StudyGroupPersistenceClient(StudyGroupClient wrappedClient, string dbConnectionString)
        {
            this.wrappedClient = wrappedClient ?? throw new ArgumentNullException(nameof(wrappedClient));
            this.dbConnectionString = 
                !string.IsNullOrWhiteSpace(dbConnectionString) 
                   ? dbConnectionString
                   : throw new ArgumentNullException(nameof(dbConnectionString));
        }

        public override HttpClient Http => this.wrappedClient.Http;

        public override async Task CreateStudyGroupAsync(CreateStudyGroupRequest request, string correlationId = null)
        {
            await base.CreateStudyGroupAsync(request, correlationId).ConfigureAwait(false);
            await this.SaveStudyGroupAsync(request.Name).ConfigureAwait(false);
        }

        public override async Task<HttpResponseMessage> TryCreateStudyGroupAsync(CreateStudyGroupRequest request, string correlationId = null)
        {
            HttpResponseMessage result = 
                await base.TryCreateStudyGroupAsync(request, correlationId).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                return result;
            }

            await this.SaveStudyGroupAsync(request.Name).ConfigureAwait(false);

            return result;
        }

        public override void Dispose()
        {
            this.CleanData();
            base.Dispose();
        }

        private void CleanData()
        {
            this.CleanDataAsync()
                .GetAwaiter()
                .GetResult();
        }

        private async Task CleanDataAsync()
        {
            if (wrappedClient == null)
            {
                return;
            }

            using var context = GetContext();

            foreach (var group in createdStudyGroups)
            {
                StudyGroup groupToDelete = 
                    await context
                        .StudyGroups
                        .FindAsync(group.StudyGroupId)
                        .ConfigureAwait(false);
                context.StudyGroups.Remove(groupToDelete);
            }

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        private StudyGroupsContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<StudyGroupsContext>();
            optionsBuilder.UseSqlServer(dbConnectionString);

            return new StudyGroupsContext(optionsBuilder.Options);
        }

        private async Task SaveStudyGroupAsync(string studyGroupName)
        {
            using var context = GetContext();
            StudyGroup group = await context
                .StudyGroups
                .FirstOrDefaultAsync(sg => sg.Name == studyGroupName)
                .ConfigureAwait(false);

            if (group != null)
            {
                this.createdStudyGroups.Add(group);
            }
        }
    }
}