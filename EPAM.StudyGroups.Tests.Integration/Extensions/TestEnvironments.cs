namespace EPAM.StudyGroups.Tests.Integration.Extensions
{
    /// <summary>
    /// Substitution for <see cref="Microsoft.Extensions.Hosting.Environments"/>.
    /// Has additional test environments.
    /// Is used to set up SUT and select test suits.
    /// </summary>
    public class TestEnvironments
    {
        public const string InMemory = "InMemory";
        public const string Development = "Development";
    }
}