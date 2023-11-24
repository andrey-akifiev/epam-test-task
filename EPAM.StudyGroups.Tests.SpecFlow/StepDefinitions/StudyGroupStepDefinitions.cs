using EPAM.StudyGroups.Api.Models;
using EPAM.StudyGroups.Data;
using EPAM.StudyGroups.Data.Models;
using EPAM.StudyGroups.Tests.Integration;
using EPAM.StudyGroups.Tests.Integration.Controllers;
using EPAM.StudyGroups.Tests.Integration.Extensions;
using EPAM.StudyGroups.Tests.Integration.Models;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TechTalk.SpecFlow;

namespace EPAM.StudyGroups.Tests.SpecFlow.StepDefinitions
{
    [Binding]
    public sealed class StudyGroupStepDefinitions
    {
        private const string TestClient = "TEST_CLIENT";
        private const string NewStudyGroupName = "NEW_STUDY_GROUP_NAME";
        private const string NewStudyGroupSubject = "NEW_STUDY_GROUP_SUBJECT";
        private const string LastResponse = "LAST_RESPONSE";
        private const string LastData = "LAST_DATA";
        private const string LastUser = "LAST_USER";

        private static readonly string apiConnectionString;
        private static readonly string dbConnectionString;

        public static readonly string CurrentEnvironemnt = EnvironmentVariables.TestEnvironment ?? TestEnvironments.Development;

        private readonly ScenarioContext scenarioContext;

        static StudyGroupStepDefinitions()
        {
            IConfigurationRoot config =
                new ConfigurationBuilder()
                    .AddJsonFile("launchSettings.json")
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{CurrentEnvironemnt}.json", true)
                    .Build();

            string connectionStrings = config.GetValue<string>("profiles:EPAM.StudyGroups.Api:applicationUrl");
            apiConnectionString = connectionStrings.Split(';')[0];

            dbConnectionString = config.GetValue<string>("ConnectionStrings:StudyGroupsContext");
        }

        public StudyGroupStepDefinitions(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            StudyGroupClient client = 
                new StudyGroupPersistenceClient(
                    new StudyGroupClient(
                        new HttpClient
                        {
                            BaseAddress = new Uri(apiConnectionString),
                        }),
                    dbConnectionString);

            this.scenarioContext[TestClient] = client;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            (this.scenarioContext[TestClient] as StudyGroupClient)?.Dispose();
        }

        [Given("I create a '(.*)' study group with '(.*)' subject")]
        [When("I create a '(.*)' study group with '(.*)' subject")]
        public async Task CreateNewStudyGroup(string groupNameType, string subject)
        {
            var client = this.scenarioContext[TestClient] as StudyGroupClient;
            HttpResponseMessage response;

            string newStudyGroupName = groupNameType;
            string newStudyGroupSubject = subject;

            switch (groupNameType.ToLower())
            {
                case "new":
                    newStudyGroupName = BaseControllerTests.GetRandomName();

                    this.scenarioContext[NewStudyGroupName] = newStudyGroupName;
                    this.scenarioContext[NewStudyGroupSubject] = newStudyGroupSubject;

                    break;
                case "existing":
                    newStudyGroupName = this.scenarioContext[NewStudyGroupName] as string;

                    break;

                case "":

                    break;
                default:
                    throw new NotImplementedException($"Creation of '{groupNameType}' study group has not been implemented yet.");
            }

            response = await client
                .TryCreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = newStudyGroupName,
                        Subject = newStudyGroupSubject,
                    })
                .ConfigureAwait(false);

            this.scenarioContext[LastResponse] = response;
        }
        
        [Given("I create a '(.*)' user")]
        [When("I create a '(.*)' user")]
        public async Task I_create_a_user(string userType)
        {
            User user = null;

            switch (userType.ToLower())
            {
                case "new":
                    using (StudyGroupsContext context = this.GetContext())
                    {
                        user = new User
                        {
                            Email = $"{BaseControllerTests.GetRandomName()}@test.com",
                            FirstName = BaseControllerTests.GetRandomName(),
                            LastName = BaseControllerTests.GetRandomName(),
                        };

                        user = (await context
                            .Users
                            .AddAsync(user)
                            .ConfigureAwait(false))
                            .Entity;

                        await context.SaveChangesAsync().ConfigureAwait(false);
                    }

                    break;
                default:
                    throw new NotImplementedException($"Creation of '{userType}' user has not been implemented yet.");
            }

            this.scenarioContext[LastUser] = user;
        }

        [When("I ask for a list of study groups")]
        [Then("I ask for a list of study groups")]
        public async Task I_ask_for_a_list_of_study_groups()
        {
            var client = this.scenarioContext[TestClient] as StudyGroupClient;

            (TestStudyGroup[] data, HttpResponseMessage response) = await client
                .TryGetStudyGroupsAsync()
                .ConfigureAwait(false);

            this.scenarioContext[LastData] = data;
            this.scenarioContext[LastResponse] = response;
        }

        [When("I search for a list of study groups by '(.*)' subject")]
        public async Task I_search_for_a_list_of_study_groups(string subject)
        {
            var client = this.scenarioContext[TestClient] as StudyGroupClient;

            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TrySearchStudyGroupsAsync(subject)
                .ConfigureAwait(false);

            this.scenarioContext[LastData] = data;
            this.scenarioContext[LastResponse] = response;
        }

        [Given("I '(.*)' a '(.*)' study group as '(.*)' user")]
        [When("I '(.*)' a '(.*)' study group as '(.*)' user")]
        public async Task I_join_to_a_study_group_as(string action, string studyGroup, string user)
        {
            var client = this.scenarioContext[TestClient] as StudyGroupClient;

            string userId = null;
            string studyGroupId = null;
            
            switch (user)
            {
                case "new":
                    userId = (this.scenarioContext[LastUser] as User).Id.ToString();
                    break;
                case "non-existing":
                    userId = int.MaxValue.ToString();
                    break;
                case "":
                    userId = string.Empty;
                    break;
            }

            switch (studyGroup)
            {
                case "new":
                    studyGroupId = (this.scenarioContext[NewStudyGroupName] as string);

                    using (StudyGroupsContext context = GetContext())
                    {
                        studyGroupId = (await context
                            .StudyGroups
                            .FirstOrDefaultAsync(g => g.Name == studyGroupId)
                            .ConfigureAwait(false))
                            .StudyGroupId
                            .ToString();
                    }

                    break;
                case "":
                    studyGroupId = string.Empty;
                    break;
                case "non-existing":
                    studyGroupId = int.MaxValue.ToString();
                    break;
            }

            switch (action.ToLower())
            {
                case "join":
                    this.scenarioContext[LastResponse] =
                        await client
                            .TryJoinStudyGroupAsync(
                                studyGroupId: studyGroupId,
                                userId: userId)
                            .ConfigureAwait(false);
                    break;
                case "leave":
                    this.scenarioContext[LastResponse] =
                        await client
                            .TryLeaveStudyGroupAsync(
                                studyGroupId: studyGroupId,
                                userId: userId)
                            .ConfigureAwait(false);
                    break;
                default:
                    throw new NotImplementedException($"Action '{action}' has not been implemented yet.");
            }
        }

        [Then("'(.*)' study group with '(.*)' subject has been created")]
        public async Task ThenTheResultShouldBe(string groupNameType, string expectedSubject)
        {
            switch (groupNameType.ToLower())
            {
                case "new":
                    var client = this.scenarioContext[TestClient] as StudyGroupClient;

                    var result = await client
                        .GetStudyGroupsAsync()
                        .ConfigureAwait(false);

                    result.Should()
                        .HaveCount(1)
                        .And
                        .Subject
                        .First()
                        .Should()
                        .BeEquivalentTo(
                            new StudyGroup
                            {
                                Name = this.scenarioContext[NewStudyGroupName] as string,
                                Subject = 
                                    Enum.Parse<Subject>(expectedSubject),
                                CreateDate = DateTime.UtcNow,
                            },
                            config => config
                                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 15.Seconds()))
                                .WhenTypeIs<DateTime>()
                                .Excluding(o => o.Users)
                                .Excluding(o => o.StudyGroupId))
                        .And
                        .Subject
                        .As<StudyGroup>()
                        .StudyGroupId
                        .Should()
                        .BeGreaterThan(0);
                    break;
                default:
                    throw new PendingStepException();
            }
        }

        [Then("'(.*)' status is returned")]
        public void ThenTheResultShouldBe(string responseType)
        {
            var client = this.scenarioContext[LastResponse] as HttpResponseMessage;

            switch (responseType.ToLower())
            {
                case "conflict":
                    client.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
                    break;
                case "badrequest":
                    client.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
                    break;
                case "notfound":
                    client.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
                    break;
                case "ok":
                    client.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                    break;
                default:
                    throw new NotImplementedException($"Processing of '{responseType}' response has not been implemented yet.");
            }
        }

        [Then("the list of study groups '(.*)'")]
        public void The_list_of_study_groups(string stateOption)
        {
            var data = this.scenarioContext[LastData] as StudyGroup[];
            var response = this.scenarioContext[LastResponse] as HttpResponseMessage;

            switch (stateOption.ToLower())
            {
                case "is empty":
                    response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                    data.Should().BeEmpty();
                    break;
                case "contains new group":
                    response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

                    data.Should()
                        .HaveCount(1)
                        .And
                        .Subject
                        .First()
                        .Should()
                        .BeEquivalentTo(
                            new StudyGroup
                            {
                                Name = this.scenarioContext[NewStudyGroupName] as string,
                                Subject = Enum.Parse<Subject>(this.scenarioContext[NewStudyGroupSubject] as string),
                                CreateDate = DateTime.UtcNow,
                            },
                            config => config
                                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 15.Seconds()))
                                .WhenTypeIs<DateTime>()
                                .Excluding(o => o.Users)
                                .Excluding(o => o.StudyGroupId))
                        .And
                        .Subject
                        .As<StudyGroup>()
                        .StudyGroupId
                        .Should()
                        .BeGreaterThan(0);
                    break;
                default:
                    throw new NotImplementedException($"'{stateOption}' state of study groups' list has not been implemented yet.");
            }
        }

        [Then("the '(.*)' study group contains '(.*)' user")]
        public void The_study_group_contains_user(string studyGroup, string userType)
        {
            var data = this.scenarioContext[LastData] as TestStudyGroup[];
            var response = this.scenarioContext[LastResponse] as HttpResponseMessage;

            string studyGroupName = null;
            User user = null;

            switch (studyGroup)
            {
                case "new":
                    studyGroupName = this.scenarioContext[NewStudyGroupName] as string;
                    break;
                default:
                    throw new PendingStepException();
            }

            switch (userType)
            {
                case "new":
                    user = this.scenarioContext[LastUser] as User;
                    break;
                default:
                    throw new PendingStepException();
            }

            data
                .Single(g => g.Name == studyGroupName)
                .Users
                .Should()
                .NotBeEmpty()
                .And
                .ContainEquivalentOf(user, config => config.Excluding(o => o.StudyGroups));
        }

        private StudyGroupsContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<StudyGroupsContext>();
            optionsBuilder.UseSqlServer(dbConnectionString);

            return new StudyGroupsContext(optionsBuilder.Options);
        }
    }
}