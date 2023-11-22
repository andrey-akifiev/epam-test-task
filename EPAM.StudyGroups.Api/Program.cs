using EPAM.StudyGroups.Data.DAL;
using EPAM.StudyGroups.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPAM.StudyGroups.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            IConfiguration configuration = builder.Configuration;
            string studyGroupsContextConnString = configuration.GetConnectionString(nameof(StudyGroupsContext));
            builder.Services.AddDbContext<StudyGroupsContext>(options =>
            {
                options.UseSqlServer(studyGroupsContextConnString);
                options.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored));
            });

            // Add services to the container.
            builder.Services.AddTransient<IStudyGroupRepository, StudyGroupRepository>();
            builder.Services.AddTransient<IUserRepository, UserRepository>();

            builder
                .Services
                .AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.ReferenceHandler = 
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}