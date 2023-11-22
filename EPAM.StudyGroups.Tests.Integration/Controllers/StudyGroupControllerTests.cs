﻿using EPAM.StudyGroups.Api.Models;
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
                        .Excluding(o => o.Users));
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
                        .Excluding(o => o.Users));
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
                        StudyGroupId = 2,
                        CreateDate = DateTime.UtcNow,
                    },
                    config => config
                        .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 15.Seconds()))
                        .WhenTypeIs<DateTime>()
                        .Excluding(o => o.Users));
        }
    }
}