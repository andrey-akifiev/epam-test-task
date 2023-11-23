using EPAM.StudyGroups.Data.DAL;
using EPAM.StudyGroups.Tests.Integration.DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace EPAM.StudyGroups.Tests.Integration
{
    public class StudyGroupsWebAppFactory : WebApplicationFactory<Api.Program>
    {
        public InMemoryTestUserRepository TestUserRepository { get; set; }
        public TestStudyGroupRepository TestStudyGroupRepository { get; set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureServices(services =>
            {
                TestStudyGroupRepository = new TestStudyGroupRepository();
                services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IStudyGroupRepository))!);
                services.AddSingleton(typeof(IStudyGroupRepository), TestStudyGroupRepository);

                TestUserRepository = new InMemoryTestUserRepository();
                services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IUserRepository))!);
                services.AddSingleton(typeof(IUserRepository), TestUserRepository);
            });
        }
    }
}