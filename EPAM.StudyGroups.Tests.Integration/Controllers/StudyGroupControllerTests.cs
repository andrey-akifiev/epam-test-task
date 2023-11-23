using EPAM.StudyGroups.Api.Models;
using EPAM.StudyGroups.Data.Models;
using FluentAssertions;
using FluentAssertions.Extensions;
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

        [Test]
        public async Task GetStudyGroups_ShouldReturnEmptyList_WhenNoData()
        {
            // ARRANGE
            using var client = this.GetStudyGroupClient();

            // ACT
            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TryGetStudyGroupsAsync()
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            data.Should().BeEmpty();
        }

        [Test]
        public async Task GetStudyGroups_ShouldReturnOneGroup_WhenOneGroupIsCreated()
        {
            // ARRANGE
            string expectedName = GetRandomName();
            Subject expectedSubject = Subject.Physics;

            using var client = this.GetStudyGroupClient();

            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedName,
                        Subject = expectedSubject.ToString(),
                    })
                .ConfigureAwait(false);

            // ACT
            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TryGetStudyGroupsAsync()
                .ConfigureAwait(false);

            // ASSERT
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
                        Name = expectedName,
                        Subject = expectedSubject,
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
        }

        [Test]
        public async Task SearchStudyGroups_ShouldReturnEmptyList_WhenNoData()
        {
            // ARRANGE
            using var client = this.GetStudyGroupClient();

            // ACT
            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TrySearchStudyGroupsAsync(Subject.Chemistry.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            data.Should().BeEmpty();
        }

        [Test]
        public async Task SearchStudyGroups_ShouldReturnEmptyList_WhenSearchMissed()
        {
            // ARRANGE
            using var client = this.GetStudyGroupClient();
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = GetRandomName(),
                        Subject = Subject.Physics.ToString(),
                    })
                .ConfigureAwait(false);

            // ACT
            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TrySearchStudyGroupsAsync(Subject.Chemistry.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            data.Should().BeEmpty();
        }

        [Test]
        public async Task SearchStudyGroups_ShouldReturnOneElement_WhenElementExists()
        {
            // ARRANGE
            string expectedName = GetRandomName();
            Subject expectedSubject = Subject.Physics;

            using var client = this.GetStudyGroupClient();
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedName,
                        Subject = expectedSubject.ToString(),
                    })
                .ConfigureAwait(false);

            // ACT
            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TrySearchStudyGroupsAsync(expectedSubject.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            data.Should()
                .HaveCount(1)
                .And
                .ContainEquivalentOf(
                    new StudyGroup
                    {
                        Name = expectedName,
                        Subject = expectedSubject,
                        StudyGroupId = 1,
                        CreateDate = DateTime.UtcNow,
                    },
                    config => config
                        .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 15.Seconds()))
                        .WhenTypeIs<DateTime>()
                        .Excluding(o => o.Users)
                        .Excluding(o => o.StudyGroupId));
        }

        [Test]
        public async Task SearchStudyGroups_ShouldReturnOneElement_WhenSearchHits()
        {
            // ARRANGE
            string expectedName = GetRandomName();
            Subject expectedSubject = Subject.Physics;

            using var client = this.GetStudyGroupClient();
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = GetRandomName(),
                        Subject = Subject.Chemistry.ToString(),
                    })
                .ConfigureAwait(false);
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedName,
                        Subject = expectedSubject.ToString(),
                    })
                .ConfigureAwait(false);

            // ACT
            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TrySearchStudyGroupsAsync(expectedSubject.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            data.Should()
                .HaveCount(1)
                .And
                .ContainEquivalentOf(
                    new StudyGroup
                    {
                        Name = expectedName,
                        Subject = expectedSubject,
                        CreateDate = DateTime.UtcNow,
                    },
                    config => config
                        .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 15.Seconds()))
                        .WhenTypeIs<DateTime>()
                        .Excluding(o => o.Users)
                        .Excluding(o => o.StudyGroupId));
        }

        [Test]
        public async Task JoinStudyGroup_ShouldReturnBadRequest_WhenStudyGroupAndUserIdAreEmpty()
        {
            // ARRANGE
            using var client = this.GetStudyGroupClient();

            // ACT
            HttpResponseMessage response = await client
                .TryJoinStudyGroupAsync(
                    studyGroupId: string.Empty,
                    userId: string.Empty)
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task JoinStudyGroup_ShouldReturnNotFound_WhenStudyGroupWithSpecifiedIdDoesNotExist()
        {
            // ARRANGE
            var expectedUser = new User
            {
                Email = $"{GetRandomName()}@test.com",
                FirstName = GetRandomName(),
                LastName = GetRandomName(),
            };

            using var client = this.GetStudyGroupClient();

            this.TestUserRepository.AddUser(expectedUser);

            // ACT
            HttpResponseMessage response = await client
                .TryJoinStudyGroupAsync(
                    studyGroupId: 
                        int.MaxValue.ToString(),
                    userId:
                        (await TestUserRepository
                            .GetUsers(CancellationToken.None)
                            .ConfigureAwait(false))
                            .First(u => u.Email == expectedUser.Email)
                            .Id
                            .ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Test]
        public async Task JoinStudyGroup_ShouldReturnNotFound_WhenUserWithSpecifiedIdDoesNotExist()
        {
            // ARRANGE
            string expectedGroupName = GetRandomName();

            using var client = this.GetStudyGroupClient();
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedGroupName,
                        Subject = Subject.Chemistry.ToString(),
                    })
                .ConfigureAwait(false);

            // ACT
            HttpResponseMessage response = await client
                .TryJoinStudyGroupAsync(
                    studyGroupId:
                        (await client
                            .GetStudyGroupsAsync()
                            .ConfigureAwait(false))
                            .First(g => g.Name == expectedGroupName)
                            .StudyGroupId
                            .ToString(),
                    userId:
                        int.MaxValue.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Test]
        public async Task JoinStudyGroup_ShouldReturnOK_WhenUserAndStudyGroupAreValid()
        {
            // ARRANGE
            var expectedUser = new User
            {
                Email = $"{GetRandomName()}@test.com",
                FirstName = GetRandomName(),
                LastName = GetRandomName(),
            };

            string expectedGroupName = GetRandomName();

            using var client = this.GetStudyGroupClient();
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedGroupName,
                        Subject = Subject.Chemistry.ToString(),
                    })
                .ConfigureAwait(false);

            this.TestUserRepository.AddUser(expectedUser);

            // ACT
            HttpResponseMessage response = await client
                .TryJoinStudyGroupAsync(
                    studyGroupId:
                        (await client
                            .GetStudyGroupsAsync()
                            .ConfigureAwait(false))
                            .First(g => g.Name == expectedGroupName)
                            .StudyGroupId
                            .ToString(),
                    userId:
                        (await TestUserRepository
                            .GetUsers(CancellationToken.None)
                            .ConfigureAwait(false))
                            .First(u => u.Email == expectedUser.Email)
                            .Id
                            .ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test]
        public async Task JoinStudyGroup_ShouldReturnConflict_WhenAlreadyJoined()
        {
            // ARRANGE
            var expectedUser = new User
            {
                Email = $"{GetRandomName()}@test.com",
                FirstName = GetRandomName(),
                LastName = GetRandomName(),
            };

            string expectedGroupName = GetRandomName();

            using var client = this.GetStudyGroupClient();
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedGroupName,
                        Subject = Subject.Chemistry.ToString(),
                    })
                .ConfigureAwait(false);

            this.TestUserRepository.AddUser(expectedUser);

            int expectedStudyGroupId =
                (await client
                    .GetStudyGroupsAsync()
                    .ConfigureAwait(false))
                    .First(g => g.Name == expectedGroupName)
                    .StudyGroupId
                    .Value;
            int expectedUserId =
                (await TestUserRepository
                    .GetUsers(CancellationToken.None)
                    .ConfigureAwait(false))
                    .First(u => u.Email == expectedUser.Email)
                    .Id;
            await client
                .JoinStudyGroupAsync(
                    studyGroupId: expectedStudyGroupId,
                    userId: expectedUserId)
                .ConfigureAwait(false);

            // ACT
            HttpResponseMessage response = await client
                .TryJoinStudyGroupAsync(
                    studyGroupId: expectedStudyGroupId.ToString(),
                    userId: expectedUserId.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
        }

        [Test]
        public async Task LeaveStudyGroup_ShouldReturnBadRequest_WhenStudyGroupAndUserIdAreEmpty()
        {
            // ARRANGE
            using var client = this.GetStudyGroupClient();

            // ACT
            HttpResponseMessage response = await client
                .TryLeaveStudyGroupAsync(
                    studyGroupId: string.Empty,
                    userId: string.Empty)
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task LeaveStudyGroup_ShouldReturnNotFound_WhenStudyGroupWithSpecifiedIdDoesNotExist()
        {
            // ARRANGE
            var expectedUser = new User
            {
                Email = $"{GetRandomName()}@test.com",
                FirstName = GetRandomName(),
                LastName = GetRandomName(),
            };

            using var client = this.GetStudyGroupClient();

            this.TestUserRepository.AddUser(expectedUser);

            // ACT
            HttpResponseMessage response = await client
                .TryLeaveStudyGroupAsync(
                    studyGroupId:
                        int.MaxValue.ToString(),
                    userId:
                        (await TestUserRepository
                            .GetUsers(CancellationToken.None)
                            .ConfigureAwait(false))
                            .First(u => u.Email == expectedUser.Email)
                            .Id
                            .ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Test]
        public async Task LeaveStudyGroup_ShouldReturnNotFound_WhenUserWithSpecifiedIdDoesNotExist()
        {
            // ARRANGE
            string expectedGroupName = GetRandomName();

            using var client = this.GetStudyGroupClient();
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedGroupName,
                        Subject = Subject.Chemistry.ToString(),
                    })
                .ConfigureAwait(false);

            // ACT
            HttpResponseMessage response = await client
                .TryLeaveStudyGroupAsync(
                    studyGroupId:
                        (await client
                            .GetStudyGroupsAsync()
                            .ConfigureAwait(false))
                            .First(g => g.Name == expectedGroupName)
                            .StudyGroupId
                            .ToString(),
                    userId:
                        int.MaxValue.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Test]
        public async Task LeaveStudyGroup_ShouldReturnOK_WhenUserAndStudyGroupAreValid()
        {
            // ARRANGE
            var expectedUser = new User
            {
                Email = $"{GetRandomName()}@test.com",
                FirstName = GetRandomName(),
                LastName = GetRandomName(),
            };

            string expectedGroupName = GetRandomName();

            using var client = this.GetStudyGroupClient();
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedGroupName,
                        Subject = Subject.Chemistry.ToString(),
                    })
                .ConfigureAwait(false);

            this.TestUserRepository.AddUser(expectedUser);

            int expectedStudyGroupId = 
                (await client
                    .GetStudyGroupsAsync()
                    .ConfigureAwait(false))
                    .First(g => g.Name == expectedGroupName)
                    .StudyGroupId
                    .Value;
            int expectedUserId =
                (await TestUserRepository
                    .GetUsers(CancellationToken.None)
                    .ConfigureAwait(false))
                    .First(u => u.Email == expectedUser.Email)
                    .Id;

            await client
                .JoinStudyGroupAsync(
                    studyGroupId: expectedStudyGroupId,
                    userId: expectedUserId)
                .ConfigureAwait(false);

            // ACT
            HttpResponseMessage response = await client
                .TryLeaveStudyGroupAsync(
                    studyGroupId: expectedStudyGroupId.ToString(),
                    userId: expectedUserId.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test]
        public async Task LeaveStudyGroup_ShouldReturnNotFound_WhenUserHasNotJoinedStudyGroup()
        {
            // ARRANGE
            var expectedUser = new User
            {
                Email = $"{GetRandomName()}@test.com",
                FirstName = GetRandomName(),
                LastName = GetRandomName(),
            };

            string expectedGroupName = GetRandomName();

            using var client = this.GetStudyGroupClient();
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedGroupName,
                        Subject = Subject.Chemistry.ToString(),
                    })
                .ConfigureAwait(false);

            this.TestUserRepository.AddUser(expectedUser);

            int expectedStudyGroupId =
                (await client
                    .GetStudyGroupsAsync()
                    .ConfigureAwait(false))
                    .First(g => g.Name == expectedGroupName)
                    .StudyGroupId
                    .Value;
            int expectedUserId =
                (await TestUserRepository
                    .GetUsers(CancellationToken.None)
                    .ConfigureAwait(false))
                    .First(u => u.Email == expectedUser.Email)
                    .Id;

            // ACT
            HttpResponseMessage response = await client
                .TryLeaveStudyGroupAsync(
                    studyGroupId: expectedStudyGroupId.ToString(),
                    userId: expectedUserId.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }
    }
}