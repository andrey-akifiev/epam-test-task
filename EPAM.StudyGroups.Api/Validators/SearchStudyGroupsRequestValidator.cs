using EPAM.StudyGroups.Api.Models;
using FluentValidation;

namespace EPAM.StudyGroups.Api.Validators
{
    public class SearchStudyGroupsRequestValidator : BaseValidator<SearchStudyGroupsRequest>
    {
        public SearchStudyGroupsRequestValidator()
        {
            // Part of implementation of AC1b:
            // The only valid Subjects are: Math, Chemistry, Physics
            RuleFor(x => x.Subject)
                .NotEmpty()
                .Must(BeOneOfEnumValues)
                .WithMessage(
                    $"Specified {nameof(SearchStudyGroupsRequest.Subject)} should be one of the following values: '{string.Join(',', this.GetAllValuesOfSubjectEnum())}'.");
        }
    }
}