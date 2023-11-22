using EPAM.StudyGroups.Api.Models;
using EPAM.StudyGroups.Data.Models;
using FluentAssertions;
using NUnit.Framework;

namespace EPAM.StudyGroups.Tests.Integration.Controllers
{
    public class StudyGroupControllerTests : BaseControllerTests
    {
        [Test]
        public async Task CreateStudyGroup_ShouldReturnOk_WhenNameIsValid()
        {
            // ARRANGE
            var expectedName = GetRandomName();

            using var client = this.GetStudyGroupClient();

            // ACT
            HttpResponseMessage response = await client
                .TryCreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedName,
                        Subject = Subject.Chemistry.ToString(),
                    })
                .ConfigureAwait(false);

            var repo = testStudyGroupRepository;

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test]
        public async Task CreateStudyGroup_ShouldReturnConflict_WhenNameIsOccupied()
        {
            // ARRANGE
            var expectedName = GetRandomName();

            using var client = this.GetStudyGroupClient();

            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedName,
                        Subject = Subject.Math.ToString(),
                    })
                .ConfigureAwait(false);

            // ACT
            HttpResponseMessage response = await client
                .TryCreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedName,
                        Subject = Subject.Chemistry.ToString(),
                    })
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
        }

        [Test]
        public async Task CreateStudyGroup_ShouldReturnConflict_WhenSubjectIsOccupied()
        {
            // ARRANGE
            var expectedSubject = Subject.Chemistry.ToString();

            using var client = this.GetStudyGroupClient();

            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = GetRandomName(),
                        Subject = expectedSubject,
                    })
                .ConfigureAwait(false);

            // ACT
            HttpResponseMessage response = await client
                .TryCreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = GetRandomName(),
                        Subject = expectedSubject,
                    })
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
        }

        [Test]
        public async Task CreateStudyGroup_ShouldReturnBadRequest_WhenFieldAreEmpty()
        {
            // ARRANGE
            using var client = this.GetStudyGroupClient();

            // ACT
            HttpResponseMessage response = await client
                .TryCreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = string.Empty,
                        Subject = string.Empty,
                    })
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}