using EPAM.StudyGroups.Api.Models;
using EPAM.StudyGroups.Api.Validators;
using EPAM.StudyGroups.Data.Models;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace EPAM.StudyGroup.Api.Tests.Validators
{
    public class SearchStudyGroupsRequestValidatorTests
    {
        [TestCaseSource(typeof(ValidatorsTestCases), nameof(ValidatorsTestCases.NullAndEmptyStringVariations))]
        public async Task Subject_ShouldReturnError_WhenSubjectIsEmpty(string invalidSubject)
        {
            // ARRANGE
            var model = new SearchStudyGroupsRequest { Subject = invalidSubject };

            // ACT
            var validator = new SearchStudyGroupsRequestValidator();
            var result = await validator.TestValidateAsync(model).ConfigureAwait(false);

            // ASSERT
            result
                .ShouldHaveValidationErrorFor(request => request.Subject)
                .WithErrorMessage($"'{nameof(SearchStudyGroupsRequest.Subject)}' must not be empty.");
        }

        [TestCase("###")]
        [TestCase("English")]
        public async Task Subject_ShouldReturnError_WhenSubjectIsOutOfRange(string invalidSubject)
        {
            // ARRANGE
            var model = new SearchStudyGroupsRequest { Subject = invalidSubject };

            // ACT
            var validator = new SearchStudyGroupsRequestValidator();
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
            var model = new SearchStudyGroupsRequest { Subject = validSubject.ToString() };

            // ACT
            var validator = new SearchStudyGroupsRequestValidator();
            var result = await validator.TestValidateAsync(model).ConfigureAwait(false);

            // ASSERT
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}