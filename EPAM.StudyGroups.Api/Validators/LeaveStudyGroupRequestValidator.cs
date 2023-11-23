using EPAM.StudyGroups.Api.Models;
using FluentValidation;

namespace EPAM.StudyGroups.Api.Validators
{
    public class LeaveStudyGroupRequestValidator : BaseValidator<LeaveStudyGroupRequest>
    {
        public LeaveStudyGroupRequestValidator()
        {
            RuleFor(x => x.StudyGroupId)
                .GreaterThan(0)
                .WithName(nameof(JoinStudyGroupRequest.StudyGroupId));
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithName(nameof(JoinStudyGroupRequest.UserId));
        }
    }
}