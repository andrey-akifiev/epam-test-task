using EPAM.StudyGroups.Api.Models;
using EPAM.StudyGroups.Data.Models;
using FluentValidation;

namespace EPAM.StudyGroups.Api.Validators
{
    public class CreateStudyGroupRequestValidator : AbstractValidator<CreateStudyGroupRequest>
    {
        public CreateStudyGroupRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotNull()
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage($"'{nameof(CreateStudyGroupRequest.Name)}' must not be empty.")
                .Length(5, 30);

            // Part of implementation of AC1b:
            // The only valid Subjects are: Math, Chemistry, Physics
            RuleFor(x => x.Subject)
                .NotEmpty()
                .Must(BeOneOfEnumValues)
                .WithMessage(
                    $"Specified {nameof(CreateStudyGroupRequest.Subject)} should be one of the following values: '{string.Join(',', this.GetAllValuesOfSubjectEnum())}'.");
        }

        private bool BeOneOfEnumValues(string subject)
        {
            return GetAllValuesOfSubjectEnum().Contains(subject);
        }

        private string[] GetAllValuesOfSubjectEnum()
        {
            return Enum.GetNames(typeof(Subject));
        }
    }
}