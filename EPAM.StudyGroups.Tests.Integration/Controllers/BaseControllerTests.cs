using EPAM.StudyGroups.Tests.Integration.DAL;
using EPAM.StudyGroups.Tests.Integration.Extensions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace EPAM.StudyGroups.Tests.Integration.Controllers
{
    public class BaseControllerTests
    {
        private StudyGroupsWebAppFactory webAppFactory;

        public readonly string CurrentEnvironemnt = EnvironmentVariables.TestEnvironment ?? TestEnvironments.InMemory;

        protected ITestUserRepository TestUserRepository => GetTestUserRepository();

        protected TestStudyGroupRepository testStudyGroupRepository => webAppFactory.TestStudyGroupRepository;

        [SetUp]
        public void SetUp()
        {
            webAppFactory = new StudyGroupsWebAppFactory();
        }

        public static string GetRandomName() => Guid.NewGuid().ToString().Substring(0, 25);

        protected Lazy<string> ContextConnectionString => 
            new Lazy<string>(() => 
            {
                IConfigurationRoot config =
                            new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddJsonFile($"appsettings.{this.CurrentEnvironemnt}.json", true)
                                .Build();

                return config.GetValue<string>("ConnectionStrings:StudyGroupsContext");
            });

        protected HttpClient GetClient()
        {
            switch (this.CurrentEnvironemnt)
            {
                case TestEnvironments.InMemory:
                    return webAppFactory.CreateClient();
                case TestEnvironments.Development:
                    IConfigurationRoot config = 
                        new ConfigurationBuilder()
                            .AddJsonFile("launchSettings.json")
                            .Build();

                    string connectionStrings = config.GetValue<string>("profiles:EPAM.StudyGroups.Api:applicationUrl");
                    string connectionString = connectionStrings.Split(';')[0];

                    return new HttpClient { BaseAddress = new Uri(connectionString) };
                default:
                    throw new NotSupportedException($"Specified environment '{this.CurrentEnvironemnt}' is not supported");
            }
        }

        protected StudyGroupClient GetStudyGroupClient()
        {
            var originalClient = new StudyGroupClient(GetClient());
            switch (this.CurrentEnvironemnt)
            {
                case TestEnvironments.InMemory:
                    return originalClient;
                case TestEnvironments.Development:
                    string connectionString = this.ContextConnectionString.Value;
                    return new StudyGroupPersistenceClient(originalClient, connectionString);
                default:
                    throw new NotSupportedException($"Specified environment '{this.CurrentEnvironemnt}' is not supported");
            }
        }

        protected ITestUserRepository GetTestUserRepository()
        {
            switch (this.CurrentEnvironemnt)
            {
                case TestEnvironments.InMemory:
                    return webAppFactory.TestUserRepository;
                case TestEnvironments.Development:
                    string connectionString = this.ContextConnectionString.Value;
                    return new DevelopmentTestUserRepository(connectionString);
                default:
                    throw new NotSupportedException($"Specified environment '{this.CurrentEnvironemnt}' is not supported");
            }
        }
    }
}