using EPAM.StudyGroups.Api.Models;
using EPAM.StudyGroups.Api.Validators;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace EPAM.StudyGroup.Api.Tests.Validators
{
    public class LeaveStudyGroupRequestValidatorTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public async Task StudyGroupId_ShouldReturnError_WhenStudyGroupIdIsNonPositive(int invalidValue)
        {
            // ARRANGE
            var model = new LeaveStudyGroupRequest { StudyGroupId = invalidValue, UserId = 1 };

            // ACT
            var validator = new LeaveStudyGroupRequestValidator();
            var result = await validator.TestValidateAsync(model).ConfigureAwait(false);

            // ASSERT
            result
                .ShouldHaveValidationErrorFor(request => request.StudyGroupId)
                .WithErrorMessage($"'{nameof(LeaveStudyGroupRequest.StudyGroupId)}' must be greater than '0'.");
        }

        [TestCase(1)]
        public async Task StudyGroupId_ShouldNotReturnError_WhenStudyGroupIdIsPositive(int validValue)
        {
            // ARRANGE
            var model = new LeaveStudyGroupRequest { StudyGroupId = validValue, UserId = 1 };

            // ACT
            var validator = new LeaveStudyGroupRequestValidator();
            var result = await validator.TestValidateAsync(model).ConfigureAwait(false);

            // ASSERT
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task UserId_ShouldReturnError_WhenUserIdIsNonPositive(int invalidValue)
        {
            // ARRANGE
            var model = new LeaveStudyGroupRequest { StudyGroupId = 1, UserId = invalidValue };

            // ACT
            var validator = new LeaveStudyGroupRequestValidator();
            var result = await validator.TestValidateAsync(model).ConfigureAwait(false);

            // ASSERT
            result
                .ShouldHaveValidationErrorFor(request => request.UserId)
                .WithErrorMessage($"'{nameof(LeaveStudyGroupRequest.UserId)}' must be greater than '0'.");
        }

        [TestCase(1)]
        public async Task UserId_ShouldNotReturnError_WhenUserIdIsPositive(int validValue)
        {
            // ARRANGE
            var model = new LeaveStudyGroupRequest { StudyGroupId = 1, UserId = validValue };

            // ACT
            var validator = new LeaveStudyGroupRequestValidator();
            var result = await validator.TestValidateAsync(model).ConfigureAwait(false);

            // ASSERT
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}