# epam-test-task

## Intro

## Restoring the app
Basically initial commit is [Copied and restored code from pdf](https://github.com/andrey-akifiev/epam-test-task/commit/849a9ced44ba2c23c5b605ad8d2bbbf26bad98fb). 
It contains code provided in task description ([PDF file](TODO: Place a link here)) as is.
Of cause it wasn't possible to write working automation against just a part of the app. That's why I had to make some changes (See more [here](https://github.com/andrey-akifiev/epam-test-task/commit/97d2827cdb8cdb85f9a6b77bd150e87ef94fc9a8?diff=unified)).
At first I decorated actions with corresponding attributes like `HttpPost` or `HttpGet`, etc. Then introduced `IStudyGroupRepository` to be able to use built-in DI and injected it to setup:
```csharp IStudyGroupRepository.cs
using EPAM.StudyGroups.Api.Models;

namespace EPAM.StudyGroups.Api.Data
{
    public interface IStudyGroupRepository
    {
        Task CreateStudyGroup(StudyGroup studyGroup);
        Task<IEnumerable<StudyGroup>> GetStudyGroups();
        Task JoinStudyGroup(int studyGroupId, int userId);
        Task LeaveStudyGroup(int studyGroupId, int userId);
        Task<IEnumerable<StudyGroup>> SearchStudyGroups(string subject);
    }
}
```
### Restoring the DB
Well, repository should base on a database, so the next step is to [introduce a separate project for DBContext](https://github.com/andrey-akifiev/epam-test-task/commit/b8ff70e20ed6b1b7d2fb03803a69a2053d2d24ad). `StudyGroup.cs` has been moved to the project, because it looks like it supposed to be a POCO (well, pretty much close it).
And I also added a new entity named `User` to be able to operate with users assigned to study groups:
```csharp User.cs
namespace EPAM.StudyGroups.Data.Models
{
    public class User
    {
        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }
    }
}
```
The next step was to [describe relations in DBContext and add migrations](https://github.com/andrey-akifiev/epam-test-task/commit/187b80e22ddb81be55323f06480aae3b13ae909c).
According to task description - mentioned MS SQL Server, - database connection was set up in DI:
```csharp
IConfiguration configuration = builder.Configuration;
    string studyGroupsContextConnString = configuration.GetConnectionString(nameof(StudyGroupsContext));
    builder.Services.AddDbContext<StudyGroupsContext>(options =>
    {
        options.UseSqlServer(studyGroupsContextConnString);
        options.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored));
    });
```
`appsettings.Development.json` got connection string at this stage:
```json
"ConnectionStrings": {
  "StudyGroupsContext": "Data Source=.; Database=StudyGroups; user id=test_login; password=test_login; MultipleActiveResultSets=true"
}
```

> [!NOTE]
> So if you want to run the app on you local machine, you have to set up the same user in your DB instance.

#### RecreateDB Script
During development and testing storage is always polluted very fast. It's very useful to have a script which can recreate DB with a single click.
To use it you need to:
- Set Powershell execution policy by running the following command in powershell
  ```powershell
  Set-ExecutionPolicy Bypass CurrentUser
  ```
- install dotnet ef tools globally by running:
  ```powershell
  dotnet tool install --global dotnet-ef
  ```
- Install [extension](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.CommandTaskRunner64) which will let you execute the needed scripts through VS 2022 UI using Task Runner Explorer. (Please check the link for troubleshooting if there is any issue)
 
```powershell RecreateDB.ps1
dotnet ef database drop --project "EPAM.StudyGroups.Api.csproj"  --context StudyGroupsContext --force
dotnet ef database update --project "EPAM.StudyGroups.Api.csproj"  --context StudyGroupsContext
```

### Restoring missing business logic
It's time to restore missing business logic in `StudyGroupRepository.cs` and move it `EPAM.StudyGroups.Data` project:
<details>
<summary>StudyGroupRepository.cs</summary>

```csharp
namespace EPAM.StudyGroups.Data.DAL
{
    public class StudyGroupRepository : IStudyGroupRepository, IDisposable
    {
        private readonly StudyGroupsContext context;

        private bool disposed = false;

        public StudyGroupRepository(StudyGroupsContext context)
        {
            this.context = context;
        }

        public async Task CreateStudyGroup(StudyGroup studyGroup)
        {
            await context
                .StudyGroups
                .AddAsync(studyGroup)
                .ConfigureAwait(false);
            await context
                .SaveChangesAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<StudyGroup>> GetStudyGroups()
        {
            return await this.context
                .StudyGroups
                .Include(g => g.Users)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task JoinStudyGroup(int studyGroupId, int userId)
        {
            (await this.context
                .StudyGroups
                .FindAsync(studyGroupId)
                .ConfigureAwait(false))
                .AddUser(await this.context
                    .Users
                    .FindAsync(userId)
                    .ConfigureAwait(false));
            await this.context
                .SaveChangesAsync()
                .ConfigureAwait(false);
        }

        public async Task LeaveStudyGroup(int studyGroupId, int userId)
        {
            (await this.context
                .StudyGroups
                .FindAsync(studyGroupId)
                .ConfigureAwait(false))
                .RemoveUser(await this.context
                    .Users
                    .FindAsync(userId)
                    .ConfigureAwait(false));
            await this.context
                .SaveChangesAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<StudyGroup>> SearchStudyGroups(string subject)
        {
            return await this.context
                .StudyGroups
                .Where(g => g.Subject == (Subject)Enum.Parse(typeof(Subject), subject))
                .ToListAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }

            this.disposed = true;
        }
    }
}
```
</details>

Basically, now we should be able to reuse our repository and integrate it to `StudyGroupContronller` like that:
```csharp StudyGroupController.cs
namespace EPAM.StudyGroups.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StudyGroupController
    {
        private readonly IStudyGroupRepository _studyGroupRepository;
        public StudyGroupController(IStudyGroupRepository studyGroupRepository)
        {
            _studyGroupRepository = studyGroupRepository;
        }

        [HttpPost()]
        public async Task<IActionResult> CreateStudyGroup(StudyGroup studyGroup)
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateStudyGroup(CreateStudyGroupRequest studyGroup)
        {
            await _studyGroupRepository.CreateStudyGroup(studyGroup);
            if ((await _studyGroupRepository
                .GetStudyGroups()
                .ConfigureAwait(false))
                .FirstOrDefault(g => g.Name == studyGroup.Name) != null)
            {
                return new ConflictResult();
            }

            await _studyGroupRepository
                .CreateStudyGroup(
                    new StudyGroup
                    {
                        Name = studyGroup.Name,
                        Subject = studyGroup.Subject,
                        CreateDate = DateTime.UtcNow,
                    })
                .ConfigureAwait(false);

            return new OkResult();
        }
```
It's obvious that we cannot use our `StudyGroup` POCO-like entity as a parameter for this action, since there were no setters in auto-properties, no default constructor (typical JSON deserializer requirements are violated), so I had to introduce another entity for such purposes. Futhermore, we should not ask for more data input then we really need, that's why number of properties has been reduced:
```csharp CreateStudyGroupRequest.cs
namespace EPAM.StudyGroups.Api.Models
{
    public class CreateStudyGroupRequest
    {
        public int Id { get; internal set; }

        public string Name { get; set; }

        public Subject Subject { get; set; }
    }
}
```
later I'd been forced to do the same with the rest of actions.

So, at this moment we became able to test at least several endpoints with Swagger.

The last remaining thing related to DB was `UserRepository` which has been added [here](https://github.com/andrey-akifiev/epam-test-task/commit/07620a523fb8d052cb5c4c4b6e4bf1c8c2544b7e#diff-d4a0aa02313d4df709d35356ecf59bf5dd8a30ab4813f59f87cb718073bc29e7).
The main idea of it was to make possible to implement logic related to join/left functionality.

```csharp
namespace EPAM.StudyGroups.Data.DAL
{
    public class UserRepository : IUserRepository, IDisposable
    {
        private readonly StudyGroupsContext context;

        private bool disposed = false;

        public UserRepository(StudyGroupsContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await this.context
                .Users
                .Include(g => g.StudyGroups)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }

            this.disposed = true;
        }
    }
}
```

### Acceptance Criteria Implementation




TODO: write about db conn

TODO: Write about commands.json

TODO: Change API signature to make it testable

