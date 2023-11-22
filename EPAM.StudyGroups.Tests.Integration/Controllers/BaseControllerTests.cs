using EPAM.StudyGroups.Tests.Integration.DAL;
using NUnit.Framework;

namespace EPAM.StudyGroups.Tests.Integration.Controllers
{
    public class BaseControllerTests
    {
        private StudyGroupsWebAppFactory webAppFactory;

        protected TestUserRepository TestUserRepository => webAppFactory.TestUserRepository;

        protected TestStudyGroupRepository testStudyGroupRepository => webAppFactory.TestStudyGroupRepository;

        [SetUp]
        public void SetUp()
        {
            webAppFactory = new StudyGroupsWebAppFactory();
        }

        public static string GetRandomName() => Guid.NewGuid().ToString().Substring(0, 25);

        protected HttpClient GetClient()
        {
            return webAppFactory.CreateClient();
        }

        protected StudyGroupClient GetStudyGroupClient()
        {
            return new StudyGroupClient(webAppFactory.CreateClient());
        }
    }
}