using EPAM.StudyGroups.Api.Models;
using EPAM.StudyGroups.Api.Validators;
using EPAM.StudyGroups.Data.Models;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace EPAM.StudyGroup.Api.Tests.Validators
{
    public class CreateStudyGroupRequestValidatorTests
    {
        [TestCaseSource(typeof(ValidatorsTestCases), nameof(ValidatorsTestCases.NullAndEmptyStringVariations))]
        public async Task Name_ShouldReturnError_WhenNameIsEmpty(string invalidName)
        {
            // ARRANGE
            var model = new CreateStudyGroupRequest { Name = invalidName, Subject = Subject.Math.ToString() };

            // ACT
            var validator = new CreateStudyGroupRequestValidator();
            var result = await validator.TestValidateAsync(model).ConfigureAwait(false);

            // ASSERT
            result
                .ShouldHaveValidationErrorFor(request => request.Name)
                .WithErrorMessage($"'{nameof(CreateStudyGroupRequest.Name)}' must not be empty.");
        }

        [TestCase(4)]
        [TestCase(31)]
        public async Task Name_ShouldReturnError_WhenNameIsOutOfLengthRange(int expectedLength)
        {
            // ARRANGE
            var model = new CreateStudyGroupRequest
                {
                    Name = new string('a', expectedLength),
                    Subject = Subject.Math.ToString(),
                };

            // ACT
            var validator = new CreateStudyGroupRequestValidator();
            var result = await validator.TestValidateAsync(model).ConfigureAwait(false);

            // ASSERT
            result
                .ShouldHaveValidationErrorFor(request => request.Name)
                .WithErrorMessage(
                    $"'{nameof(CreateStudyGroupRequest.Name)}' must be between 5 and 30 characters. You entered {expectedLength} characters.");
        }

        [TestCase(5)]
        [TestCase(30)]
        public async Task Name_ShouldNotReturnError_WhenNameIsInLengthRange(int expectedLength)
        {
            // ARRANGE
            var model = new CreateStudyGroupRequest
            {
                Name = new string('a', expectedLength),
                Subject = Subject.Math.ToString(),
            };

            // ACT
            var validator = new CreateStudyGroupRequestValidator();
            var result = await validator.TestValidateAsync(model).ConfigureAwait(false);

            // ASSERT
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestCaseSource(typeof(ValidatorsTestCases), nameof(ValidatorsTestCases.NullAndEmptyStringVariations))]
        public async Task Subject_ShouldReturnError_WhenSubjectIsEmpty(string invalidSubject)
        {
            // ARRANGE
            var model = new CreateStudyGroupRequest { Name = new string('a', 5), Subject = invalidSubject };

            // ACT
            var validator = new CreateStudyGroupRequestValidator();
            var result = await validator.TestValidateAsync(model).ConfigureAwait(false);

            // ASSERT
            result
                .ShouldHaveValidationErrorFor(request => request.Subject)
                .WithErrorMessage($"'{nameof(CreateStudyGroupRequest.Subject)}' must not be empty.");
        }

        [TestCase("###")]
        [TestCase("English")]
        public async Task Subject_ShouldReturnError_WhenSubjectIsOutOfRange(string invalidSubject)
        {
            // ARRANGE
            var model = new CreateStudyGroupRequest { Name = new string('a', 5), Subject = invalidSubject };

            // ACT
            var validator = new CreateStudyGroupRequestValidator();
            var result = await validator.TestValidateAsync(model).ConfigureAwait(false);

            // ASSERT
            result
                .ShouldHaveValidationErrorFor(request => request.Subject)
                .WithErrorMessage(
                    "Specified Subject should be one of the following values: '"
                    + string.Join(',', Subject.Math.ToString(), Subject.Chemistry.ToString(), Subject.Physics.ToString())
                    + "'.");
        }

        [TestCase(Subject.Math)]
        public async Task Subject_ShouldNotReturnError_WhenSubjectIsInRange(Subject validSubject)
        {
            // ARRANGE
            var model = new CreateStudyGroupRequest { Name = new string('a', 5), Subject = validSubject.ToString() };

            // ACT
            var validator = new CreateStudyGroupRequestValidator();
            var result = await validator.TestValidateAsync(model).ConfigureAwait(false);

            // ASSERT
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}