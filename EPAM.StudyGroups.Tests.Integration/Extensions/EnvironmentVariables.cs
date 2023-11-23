using System.Collections;

namespace EPAM.StudyGroups.Tests.Integration.Extensions
{
    public class EnvironmentVariables
    {
        private static readonly Dictionary<string, string> sessionVariables;
        static EnvironmentVariables()
        {
            // Single interop call here - aa.
            var currentEnvironmentVariables = Environment.GetEnvironmentVariables();
            sessionVariables = new Dictionary<string, string>();

            foreach (DictionaryEntry item in currentEnvironmentVariables)
            {
                sessionVariables.Add(item.Key as string, item.Value as string);
            }
        }

        // TESTING SETTINGS
        public const string TestEnvironmentVariable = "TEST_ENVIRONMENT";

        public static string TestEnvironment => GetEnvironmentVariableValue(TestEnvironmentVariable);

        private static string GetEnvironmentVariableValue(string environmentVariableName)
        {
            if (!sessionVariables.TryGetValue(environmentVariableName, out string environmentVariableValue))
            {
                throw new ArgumentNullException(
                    $"Check that [{environmentVariableName}] environment variable is set up properly.",
                    environmentVariableName);
            }

            return environmentVariableValue;
        }
    }
}