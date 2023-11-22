using FluentValidation.AspNetCore;
using FluentValidation;
using System.Reflection;

namespace EPAM.StudyGroups.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFluentValidation(this IServiceCollection services)
        {
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) =>
                ValidatorOptions.Global.PropertyNameResolver(type, memberInfo, expression);

            services.AddFluentValidationAutoValidation();

            var assemblies = AppDomain
                 .CurrentDomain
                 .GetAssemblies()
                 .Where(a => a.GetName().Name.StartsWith("EPAM.StudyGroup")
                    && !a.GetName().Name.Contains("Tests"));

            foreach (Assembly assembly in assemblies)
            {
                services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped);
            }

            return services;
        }

        private static bool IsDerivedOfGenericType(Type type, Type genericType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                return true;
            if (type.BaseType != null)
            {
                return IsDerivedOfGenericType(type.BaseType, genericType);
            }
            return false;
        }
    }
}
