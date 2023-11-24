using EPAM.StudyGroups.Data.Models;
using EPAM.StudyGroups.Tests.Integration;
using EPAM.StudyGroups.Tests.Integration.Models;
using Microsoft.Extensions.Configuration;
using NBomber.CSharp;

namespace EPAM.StudyGroups.Tests.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfigurationRoot config =
                new ConfigurationBuilder()
                    .AddJsonFile("launchSettings.json")
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                    .Build();

            string connectionStrings = config.GetValue<string>("profiles:EPAM.StudyGroups.Api:applicationUrl");
            string apiConnectionString = connectionStrings.Split(';')[0];

            string dbConnectionString = config.GetValue<string>("ConnectionStrings:StudyGroupsContext");

            using StudyGroupClient client = new StudyGroupClient(new HttpClient { BaseAddress = new Uri(apiConnectionString) });

            var scenario = Scenario.Create("http_scenario", async context =>
            {
                await Step.Run("Get study groups", context, async () => 
                {
                    (TestStudyGroup[] data, HttpResponseMessage response) = 
                        await client.TryGetStudyGroupsAsync().ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        return Response.Ok(payload: data, response.StatusCode.ToString());
                    }

                    return Response.Fail<TestStudyGroup[]>(statusCode: response.StatusCode.ToString());
                });

                await Step.Run("Search study groups", context, async () =>
                {
                    (StudyGroup[] data, HttpResponseMessage response) =
                        await client.TrySearchStudyGroupsAsync("Math").ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        return Response.Ok(payload: data, response.StatusCode.ToString());
                    }

                    return Response.Fail<StudyGroup[]>(statusCode: response.StatusCode.ToString());
                });

                return Response.Ok();
            })
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.Inject(rate: 100,
                                  interval: TimeSpan.FromSeconds(1),
                                  during: TimeSpan.FromSeconds(30))
            );

            NBomberRunner
                .RegisterScenarios(scenario)
                .Run();
        }
    }
}