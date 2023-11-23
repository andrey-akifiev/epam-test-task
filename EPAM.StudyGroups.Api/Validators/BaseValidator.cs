using EPAM.StudyGroups.Data.Models;
using FluentValidation;

namespace EPAM.StudyGroups.Api.Validators
{
    public abstract class BaseValidator<T> : AbstractValidator<T>
    {
        protected bool BeOneOfEnumValues(string subject)
        {
            return GetAllValuesOfSubjectEnum().Contains(subject);
        }

        protected string[] GetAllValuesOfSubjectEnum()
        {
            return Enum.GetNames(typeof(Subject));
        }
    }
}