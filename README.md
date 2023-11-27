# epam-test-task

## Intro

## Restoring the app
Basically initial commit is [Copied and restored code from pdf](https://github.com/andrey-akifiev/epam-test-task/commit/849a9ced44ba2c23c5b605ad8d2bbbf26bad98fb). 
It contains code provided in task description ([PDF file](https://github.com/andrey-akifiev/epam-test-task/blob/main/Test%20task.pdf)) as is.
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
Well, repository should base on a database, so the next step is to [introduce a separate project for DBContext](https://github.com/andrey-akifiev/epam-test-task/commit/b8ff70e20ed6b1b7d2fb03803a69a2053d2d24ad). `StudyGroup.cs` has been moved to this new `EPAM.StudyGroups.Data` project, because it looks like it supposed to be a POCO (well, pretty much close it).
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
During development and testing the storage is always polluted too fast. It's very useful to have a script which can recreate DB with a single click.
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

#### commands.json
Moreover, I added a `commands.json` file to be able to run the script with tow clicks from my Visual Studio. Check the gif out to know how to use it:
```json commands.json
{
  "commands": {
    "RecreateDB": {
      "fileName": "powershell.exe",
      "workingDirectory": ".",
      "arguments": "-ExecutionPolicy Bypass -NonInteractive -File Scripts\\RecreateDB.ps1"
    }
  }
}
```
TODO: ADD GIF WITH TASK RUNNER EXPLORER

### Restoring missing business logic
It's time to restore missing business logic in `StudyGroupRepository.cs` and move it to `EPAM.StudyGroups.Data` project:
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

Basically, then I was able to reuse the repository and integrate it to `StudyGroupContronller` like that:
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
    ...
    }
}
```
It's obvious that I cannot use `StudyGroup` POCO-like entity as a parameter for this action, since there were no setters in auto-properties, no default constructor (typical JSON deserializer requirements were violated), so I had to introduce another entity for such purposes. Futhermore, I should not ask for more data input then I really need, that's why number of properties has been reduced:
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

So, at this moment I became able to test at least several endpoints with Swagger.

The last remaining thing related to DB was `UserRepository` which has been added [here](https://github.com/andrey-akifiev/epam-test-task/commit/07620a523fb8d052cb5c4c4b6e4bf1c8c2544b7e#diff-d4a0aa02313d4df709d35356ecf59bf5dd8a30ab4813f59f87cb718073bc29e7).
The main idea of it was to make it possible to implement logic related to join/left functionality.

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
Finally, I was able to implement requirements in the app. I've started with AC1, which factually was the most complicated because according to the original task users can create only three study groups, one per each subject. Basically, this requirement has predetermined the upcoming design and development.
> Users are able to create only one Study Group for a single Subject

What does it mean:
- Not only names of study groups should be unique, but subjects should be as well
- Comprehencive testing is not possible without clearing data after each test, because pull of possible study groups options is limited

At first [I had](https://github.com/andrey-akifiev/epam-test-task/commit/63e30153bbd82070ca9d21424cb1fa7e18251a4e) to add a validator for the request. There are two popular options to do so: [Microsoft's built-in validators](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-8.0) based on property decoration and [FluentValidation](https://docs.fluentvalidation.net/en/latest/). Since the second way is still very popular and more challengable (from my POV), I have chosen `FluentValidation` even despite the fact that I prefer `Model validation`. Here you can see an example of one of the validators, which has to check that the `Name` porperty is a valid string of the pre-defined length and the `Subject` is a string convertible to one from the pre-defined list of subjects.

```csharp CreateStudyGroupRequestValidator.cs
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
```
Of cause, this code should be tested as well as everything else. So I'd to create a set of unit tests for the validator. According to the task I used [NUnit](https://nunit.org/) to do that.
<details>
<summary>CreateStudyGroupRequestValidatorTests.cs</summary>

```csharp CreateStudyGroupRequestValidatorTests.cs
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
```
</details>

`StudyGroupController` also has been changed to reflect the requirements. `CreateStudyGroup` got a new route - `studygroups\create`, it should return `HTTP409Conflict` in case if the specified name or subject is already occupied. And it also has some trivial mapping from DTO used as an external contract to POCO used as a contract of the repo and storage.
```csharp
namespace EPAM.StudyGroups.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StudyGroupController
    {
        private readonly IStudyGroupRepository _studyGroupRepository;
        private readonly IUserRepository _userRepository;
        public StudyGroupController(
            IStudyGroupRepository studyGroupRepository,
            IUserRepository userRepository)
        {
            _studyGroupRepository = studyGroupRepository ?? throw new ArgumentNullException(nameof(studyGroupRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateStudyGroup(CreateStudyGroupRequest studyGroup)
        {
            var newGroup = new StudyGroup
            {
                Name = studyGroup.Name,
                // Part of implementation of AC1b:
                // The only valid Subjects are: Math, Chemistry, Physics
                Subject = (Subject)Enum.Parse(typeof(Subject), studyGroup.Subject),
                // AC1c: We want to record when Study Groups were created
                CreateDate = DateTime.UtcNow,
            };

            IEnumerable<StudyGroup> groups = await _studyGroupRepository
                .GetStudyGroups()
                .ConfigureAwait(false);

            if (groups.FirstOrDefault(g => g.Name == newGroup.Name) != null)
            {
                return new ConflictResult();
            }

            // AC1: Users are able to create only one Study Group for a single Subject
            if (groups.FirstOrDefault(g => g.Subject == newGroup.Subject) != null)
            {
                return new ConflictResult();
            }

            await _studyGroupRepository
                .CreateStudyGroup(newGroup)
                .ConfigureAwait(false);

            return new OkResult();
        }
        ...
    }
}
```
## First integration tests
When the very first endpoint has been ready, I created a dedicated project for integration tests `EPAM.StudyGroups.Tests.Integration` designed to contain tests based on `WebApplicationFactory` ([see more info about](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0)). In short, it allows to run test server in memory having complete access to DI container, substituting stuff with mocks, simulating different circumstances and events etc. Again, according to the task this project is also uses NUnit. The idea of testing everything in memory like a regular unit test is to dramatically decrease feedback loop time, because the app is running without any external dependency. In this project there is only one external dependency - the DB. To be able to isolate the app from the DB it is possible to use [Test Doubles](https://martinfowler.com/bliki/TestDouble.html) technique. The only thing left to adopt it was to decide on which level it should be implemented. It could be mock of the DBContext as proposed by [EF documentation](https://learn.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking) - and that's exactly what I use for my current project, - and [mock of the repositories](https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database). This time I've chosen the second option, because the app is pretty simple and mocking of DBContext could be comparatively difficult since tester has to implement a lot of methods to make it working properly. One more important point here: since behavior of in-memory DB could be different to a real DB, there was no sense to implement in-memory database, so I just used typical generic collections.
Here you can see a test double for `StudyGroupRepository` based on `ConcurrentDictionary` with auto-PK emulation.
```csharp TestStudyGroupRepository.cs
namespace EPAM.StudyGroups.Tests.Integration.DAL
{
    public class TestStudyGroupRepository : IStudyGroupRepository
    {
        private int groupsCounter = 0;

        private ConcurrentDictionary<int, StudyGroup> studyGroups { get; init; } = new();

        private List<Tuple<int, int>> usersStudyGroups { get; init; } = new();

        public Task CreateStudyGroup(StudyGroup studyGroup)
        {
            studyGroup.StudyGroupId = ++this.groupsCounter;

            this.studyGroups.TryAdd(studyGroup.StudyGroupId.Value, studyGroup);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<StudyGroup>> GetStudyGroups()
        {
            return Task.FromResult<IEnumerable<StudyGroup>>(this.studyGroups.Values.ToList());
        }

        public Task JoinStudyGroup(int studyGroupId, int userId)
        {
            this.usersStudyGroups.Add(new (studyGroupId, userId));

            return Task.CompletedTask;
        }

        public Task LeaveStudyGroup(int studyGroupId, int userId)
        {
            this.usersStudyGroups.RemoveAll(v => v.Item1 == studyGroupId && v.Item2 == userId);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<StudyGroup>> SearchStudyGroups(string subject)
        {
            return Task.FromResult(
                this.studyGroups
                    .Where(g => g.Value.Subject == (Subject)Enum.Parse(typeof(Subject), subject))
                    .Select(g => g.Value));
        }
    }
}
```
`WebApplicationFactory` allows to use regular `HttpClient` to test the app in-memory, and this makes it possible to reuse completely the same code for both in-memory and real environment testing. So the same test could be executed as integration (unit) and system test. All the code related to handling endpoints I've placed in `StudyGroupClient.cs`.
```csharp
namespace EPAM.StudyGroups.Tests.Integration
{
    public class StudyGroupClient : IDisposable
    {
        private readonly HttpClient httpClient;

        public StudyGroupClient(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task CreateStudyGroupAsync(CreateStudyGroupRequest request, string correlationId = null)
        {
            return this.TryCreateStudyGroupAsync(request, correlationId);
        }

        public Task<HttpResponseMessage> TryCreateStudyGroupAsync(
            CreateStudyGroupRequest request,
            string correlationId = null)
        {
            return this.httpClient.TryPostAsync("/studygroup/create", request, correlationId);
        }

        public void Dispose()
        {
            this.httpClient?.Dispose();
        }

        private async Task<TResponse> FromTryMethodAsync<TResponse>(Func<Task<(TResponse, HttpResponseMessage)>> func)
        {
            (var data, var response) = await func().ConfigureAwait(false);

            data.Should().NotBeNull(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            return data;
        }
    }
}
```
As you can see, I've created two different methods to do the same job: `CreateStudyGroupAsync` and `TryCreateStudyGroupAsync`. The first one has been supposed to be used only for positive scenarios when I don't care about the exact value returned from the SUT, I only want to ensure that remote call has been executed successfully. The second method is for the opposite cases - when I want to know everything about the particular response, what is useful for negative testing. It could sound to you as [Try/Can pattern](https://startbigthinksmall.wordpress.com/2011/05/12/the-trycan-pattern/) also known as [TryParse pattern](https://blog.ploeh.dk/2019/07/15/tester-doer-isomorphisms/), but generally it should just return Tuple of several objects if execution is completed in the same way as it works in more functional languages like [JS](https://www.javascripttutorial.net/javascript-return-multiple-values/) or [Golang](https://gobyexample.com/multiple-return-values). Later, you'll see how I use it. Now let's take a look on integration tests for the very first endpoint of StudyGroup controller.

<details>
<summary>StudyGroupControllerTests.cs</summary>

```csharp StudyGroupControllerTests.cs
namespace EPAM.StudyGroups.Tests.Integration.Controllers
{
    public class StudyGroupControllerTests : BaseControllerTests
    {
        [Test]
        public async Task CreateStudyGroup_ShouldReturnOk_WhenNameIsValid()
        {
            // ARRANGE
            var expectedName = GetRandomName();

            using var client = this.GetStudyGroupClient();

            // ACT
            HttpResponseMessage response = await client
                .TryCreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedName,
                        Subject = Subject.Chemistry.ToString(),
                    })
                .ConfigureAwait(false);

            var repo = testStudyGroupRepository;

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test]
        public async Task CreateStudyGroup_ShouldReturnConflict_WhenNameIsOccupied()
        {
            // ARRANGE
            var expectedName = GetRandomName();

            using var client = this.GetStudyGroupClient();

            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedName,
                        Subject = Subject.Math.ToString(),
                    })
                .ConfigureAwait(false);

            // ACT
            HttpResponseMessage response = await client
                .TryCreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedName,
                        Subject = Subject.Chemistry.ToString(),
                    })
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
        }

        [Test]
        public async Task CreateStudyGroup_ShouldReturnConflict_WhenSubjectIsOccupied()
        {
            // ARRANGE
            var expectedSubject = Subject.Chemistry.ToString();

            using var client = this.GetStudyGroupClient();

            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = GetRandomName(),
                        Subject = expectedSubject,
                    })
                .ConfigureAwait(false);

            // ACT
            HttpResponseMessage response = await client
                .TryCreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = GetRandomName(),
                        Subject = expectedSubject,
                    })
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
        }

        [Test]
        public async Task CreateStudyGroup_ShouldReturnBadRequest_WhenFieldAreEmpty()
        {
            // ARRANGE
            using var client = this.GetStudyGroupClient();

            // ACT
            HttpResponseMessage response = await client
                .TryCreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = string.Empty,
                        Subject = string.Empty,
                    })
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
```
</details>

Here I used the same [AAA-pattern](https://medium.com/@pjbgf/title-testing-code-ocd-and-the-aaa-pattern-df453975ab80) well known in unit-testing. Names for tests are written following `MethodName_ExpectedBehavior_StateUnderTest` convention. Both negative and positive scenarios are combined in the same file representing tests for `StudyGroupController`.

## TDD for the rest of actions
TODO: Write some intro

<details>
<summary>Changes made in StudyGroupControllerTests.cs</summary>

```csharp StudyGroupControllerTests.cs
namespace EPAM.StudyGroups.Tests.Integration.Controllers
{
    public class StudyGroupControllerTests : BaseControllerTests
    {
        ...
        [Test]
        public async Task GetStudyGroups_ShouldReturnEmptyList_WhenNoData()
        {
            // ARRANGE
            using var client = this.GetStudyGroupClient();

            // ACT
            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TryGetStudyGroupsAsync()
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            data.Should().BeEmpty();
        }

        [Test]
        public async Task GetStudyGroups_ShouldReturnOneGroup_WhenOneGroupIsCreated()
        {
            // ARRANGE
            string expectedName = GetRandomName();
            Subject expectedSubject = Subject.Physics;

            using var client = this.GetStudyGroupClient();

            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedName,
                        Subject = expectedSubject.ToString(),
                    })
                .ConfigureAwait(false);

            // ACT
            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TryGetStudyGroupsAsync()
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            data.Should()
                .HaveCount(1)
                .And
                .ContainEquivalentOf(
                    new StudyGroup
                    {
                        Name = expectedName,
                        Subject = expectedSubject,
                        StudyGroupId = 1,
                        CreateDate = DateTime.UtcNow,
                    }, 
                    config => config
                        .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 15.Seconds()))
                        .WhenTypeIs<DateTime>()
                        .Excluding(o => o.Users));
        }

        [Test]
        public async Task SearchStudyGroups_ShouldReturnEmptyList_WhenNoData()
        {
            // ARRANGE
            using var client = this.GetStudyGroupClient();

            // ACT
            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TrySearchStudyGroupsAsync(Subject.Chemistry.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            data.Should().BeEmpty();
        }

        [Test]
        public async Task SearchStudyGroups_ShouldReturnEmptyList_WhenSearchMissed()
        {
            // ARRANGE
            using var client = this.GetStudyGroupClient();
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = GetRandomName(),
                        Subject = Subject.Physics.ToString(),
                    })
                .ConfigureAwait(false);

            // ACT
            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TrySearchStudyGroupsAsync(Subject.Chemistry.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            data.Should().BeEmpty();
        }

        [Test]
        public async Task SearchStudyGroups_ShouldReturnOneElement_WhenElementExists()
        {
            // ARRANGE
            string expectedName = GetRandomName();
            Subject expectedSubject = Subject.Physics;

            using var client = this.GetStudyGroupClient();
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedName,
                        Subject = expectedSubject.ToString(),
                    })
                .ConfigureAwait(false);

            // ACT
            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TrySearchStudyGroupsAsync(expectedSubject.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            data.Should()
                .HaveCount(1)
                .And
                .ContainEquivalentOf(
                    new StudyGroup
                    {
                        Name = expectedName,
                        Subject = expectedSubject,
                        StudyGroupId = 1,
                        CreateDate = DateTime.UtcNow,
                    },
                    config => config
                        .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 15.Seconds()))
                        .WhenTypeIs<DateTime>()
                        .Excluding(o => o.Users));
        }

        [Test]
        public async Task SearchStudyGroups_ShouldReturnOneElement_WhenSearchHits()
        {
            // ARRANGE
            string expectedName = GetRandomName();
            Subject expectedSubject = Subject.Physics;

            using var client = this.GetStudyGroupClient();
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = GetRandomName(),
                        Subject = Subject.Chemistry.ToString(),
                    })
                .ConfigureAwait(false);
            await client
                .CreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = expectedName,
                        Subject = expectedSubject.ToString(),
                    })
                .ConfigureAwait(false);

            // ACT
            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TrySearchStudyGroupsAsync(expectedSubject.ToString())
                .ConfigureAwait(false);

            // ASSERT
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            data.Should()
                .HaveCount(1)
                .And
                .ContainEquivalentOf(
                    new StudyGroup
                    {
                        Name = expectedName,
                        Subject = expectedSubject,
                        StudyGroupId = 2,
                        CreateDate = DateTime.UtcNow,
                    },
                    config => config
                        .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 15.Seconds()))
                        .WhenTypeIs<DateTime>()
                        .Excluding(o => o.Users));
        }
        ...
    }
}
```
</details>

These tests forced me [to change](https://github.com/andrey-akifiev/epam-test-task/commit/1a96ee32c83951bf9b9c26133643c5acaaed6463) the extarnal contract. At first to be able to implement validations for these actions. With `FluentValidation` it's not possible to write validation against primitive type like `int` or `string`, so developer has to wrap parameters with a specific type. Below you can find such a wrapper used for `studygroups/search` endpoint.

```csharp SearchStudyGroupsRequest.cs
namespace EPAM.StudyGroups.Api.Models
{
    public class SearchStudyGroupsRequest
    {
        [FromQuery(Name = nameof(Subject))]
        public string Subject { get; set; }
    }
}
```

Validator looks quite simple as well:

```csharp
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
```

Again covered with unit tests:

```csharp SearchStudyGroupsRequestValidatorTests.cs
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
```

After all I had to change the controller:

```diff
 namespace EPAM.StudyGroups.Api.Controllers
 {
     ...
     public class StudyGroupController
     {
         ...
-        [HttpGet("{subject}")]
+        [HttpGet("search")]
         [ProducesResponseType(StatusCodes.Status200OK)]
-        public async Task<IActionResult> SearchStudyGroups(string subject, CancellationToken ctn)
+        public async Task<IActionResult> SearchStudyGroups([FromQuery] SearchStudyGroupsRequest request, CancellationToken ctn)
         {
             var studyGroups = await _studyGroupRepository
-                .SearchStudyGroups(subject, ctn)
+                .SearchStudyGroups(request.Subject, ctn)
                 .ConfigureAwait(false);

             return new OkObjectResult(studyGroups);
         }
         ...
     }
 }
```

You can check integration tests for join/leave features [here](https://github.com/andrey-akifiev/epam-test-task/commit/1f7a5a52d59cbd939f53b117900b90aed0cf5d7d), it's also display how the product code has been changed regarding new tests.

## Magical instrumentation: one test code for all environments

Finally, I had to [introduce an option to run integration tests against real environment](https://github.com/andrey-akifiev/epam-test-task/commit/b05099b970a97d11c0cc1d852166bc350de1557f).

### Supported configurations

```csharp TestEnvironments.cs
﻿namespace EPAM.StudyGroups.Tests.Integration.Extensions
{
    /// <summary>
    /// Substitution for <see cref="Microsoft.Extensions.Hosting.Environments"/>.
    /// Has additional test environments.
    /// Is used to set up SUT and select test suits.
    /// </summary>
    public class TestEnvironments
    {
        public const string InMemory = "InMemory";
        public const string Development = "Development";
    }
}
```

### Runsettings

```xml InMemory.runsettings
﻿<RunSettings>
  <RunConfiguration>
    <EnvironmentVariables>
      <!-- List of environment variables we want to set-->
	  <ASPNETCORE_ENVIRONMENT>Development</ASPNETCORE_ENVIRONMENT>
      <TEST_ENVIRONMENT>InMemory</TEST_ENVIRONMENT>
    </EnvironmentVariables>
  </RunConfiguration>
</RunSettings>
```

```xml Development.runsettings
﻿<RunSettings>
  <RunConfiguration>
    <EnvironmentVariables>
      <!-- List of environment variables we want to set-->
	  <ASPNETCORE_ENVIRONMENT>Development</ASPNETCORE_ENVIRONMENT>
      <TEST_ENVIRONMENT>Development</TEST_ENVIRONMENT>
    </EnvironmentVariables>
  </RunConfiguration>
  <NUnit>
	<NumberOfTestWorkers>1</NumberOfTestWorkers>
  </NUnit>
</RunSettings>
```

### Decorator for StudyGroupClient


<details>
<summary>StudyGroupPersistenceClient.cs</summary>

```csharp StudyGroupPersistenceClient.cs
namespace EPAM.StudyGroups.Tests.Integration
{
    public class StudyGroupPersistenceClient : StudyGroupClient, IDisposable
    {
        private readonly StudyGroupClient wrappedClient;

        private readonly string dbConnectionString;

        private readonly List<StudyGroup> createdStudyGroups = new List<StudyGroup>();

        public StudyGroupPersistenceClient(StudyGroupClient wrappedClient, string dbConnectionString)
        {
            this.wrappedClient = wrappedClient ?? throw new ArgumentNullException(nameof(wrappedClient));
            this.dbConnectionString = 
                !string.IsNullOrWhiteSpace(dbConnectionString) 
                   ? dbConnectionString
                   : throw new ArgumentNullException(nameof(dbConnectionString));
        }

        public override HttpClient Http => this.wrappedClient.Http;

        public override async Task CreateStudyGroupAsync(CreateStudyGroupRequest request, string correlationId = null)
        {
            await base.CreateStudyGroupAsync(request, correlationId).ConfigureAwait(false);
            await this.SaveStudyGroupAsync(request.Name).ConfigureAwait(false);
        }

        public override async Task<HttpResponseMessage> TryCreateStudyGroupAsync(CreateStudyGroupRequest request, string correlationId = null)
        {
            HttpResponseMessage result = 
                await base.TryCreateStudyGroupAsync(request, correlationId).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                return result;
            }

            await this.SaveStudyGroupAsync(request.Name).ConfigureAwait(false);

            return result;
        }

        public override void Dispose()
        {
            this.CleanData();
            base.Dispose();
        }

        private void CleanData()
        {
            this.CleanDataAsync()
                .GetAwaiter()
                .GetResult();
        }

        private async Task CleanDataAsync()
        {
            if (wrappedClient == null)
            {
                return;
            }

            using var context = GetContext();

            foreach (var group in createdStudyGroups)
            {
                StudyGroup groupToDelete = 
                    await context
                        .StudyGroups
                        .FindAsync(group.StudyGroupId)
                        .ConfigureAwait(false);
                context.StudyGroups.Remove(groupToDelete);
            }

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        private StudyGroupsContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<StudyGroupsContext>();
            optionsBuilder.UseSqlServer(dbConnectionString);

            return new StudyGroupsContext(optionsBuilder.Options);
        }

        private async Task SaveStudyGroupAsync(string studyGroupName)
        {
            using var context = GetContext();
            StudyGroup group = await context
                .StudyGroups
                .FirstOrDefaultAsync(sg => sg.Name == studyGroupName)
                .ConfigureAwait(false);

            if (group != null)
            {
                this.createdStudyGroups.Add(group);
            }
        }
    }
}
```

</details>

### Factories for different environments

```csharp
namespace EPAM.StudyGroups.Tests.Integration.Controllers
{
    public class BaseControllerTests
    {
        private StudyGroupsWebAppFactory webAppFactory;

        public readonly string CurrentEnvironemnt = EnvironmentVariables.TestEnvironment ?? TestEnvironments.InMemory;

        protected ITestUserRepository TestUserRepository => GetTestUserRepository();

        protected TestStudyGroupRepository testStudyGroupRepository => webAppFactory.TestStudyGroupRepository;

        [SetUp]
        public void SetUp()
        {
            webAppFactory = new StudyGroupsWebAppFactory();
        }

        public static string GetRandomName() => Guid.NewGuid().ToString().Substring(0, 25);

        protected Lazy<string> ContextConnectionString => 
            new Lazy<string>(() => 
            {
                IConfigurationRoot config =
                            new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddJsonFile($"appsettings.{this.CurrentEnvironemnt}.json", true)
                                .Build();
                return config.GetValue<string>("ConnectionStrings:StudyGroupsContext");
            });

        protected HttpClient GetClient()
        {
            switch (this.CurrentEnvironemnt)
            {
                case TestEnvironments.InMemory:
                    return webAppFactory.CreateClient();
                case TestEnvironments.Development:
                    IConfigurationRoot config = 
                        new ConfigurationBuilder()
                            .AddJsonFile("launchSettings.json")
                            .Build();

                    string connectionStrings = config.GetValue<string>("profiles:EPAM.StudyGroups.Api:applicationUrl");
                    string connectionString = connectionStrings.Split(';')[0];

                    return new HttpClient { BaseAddress = new Uri(connectionString) };
                default:
                    throw new NotSupportedException($"Specified environment '{this.CurrentEnvironemnt}' is not supported");
            }
        }

        protected StudyGroupClient GetStudyGroupClient()
        {
            return new StudyGroupClient(webAppFactory.CreateClient());
            var originalClient = new StudyGroupClient(GetClient());
            switch (this.CurrentEnvironemnt)
            {
                case TestEnvironments.InMemory:
                    return originalClient;
                case TestEnvironments.Development:
                    string connectionString = this.ContextConnectionString.Value;
                    return new StudyGroupPersistenceClient(originalClient, connectionString);
                default:
                    throw new NotSupportedException($"Specified environment '{this.CurrentEnvironemnt}' is not supported");
            }
        }

        protected ITestUserRepository GetTestUserRepository()
        {
            switch (this.CurrentEnvironemnt)
            {
                case TestEnvironments.InMemory:
                    return webAppFactory.TestUserRepository;
                case TestEnvironments.Development:
                    string connectionString = this.ContextConnectionString.Value;
                    return new DevelopmentTestUserRepository(connectionString);
                default:
                    throw new NotSupportedException($"Specified environment '{this.CurrentEnvironemnt}' is not supported");
            }
        }
    }
}
```

## BDD, ATDD and fun-driven-development

```gherkin
Feature: StudyGroup

Scenario: Sucessfully create a new study group
	When I create a 'new' study group with 'Math' subject
	Then 'new' study group with 'Math' subject has been created

Scenario: Create a new study group with occupied name
   Given I create a 'new' study group with 'Math' subject
	When I create a 'existing' study group with 'Chemistry' subject
	Then 'Conflict' status is returned

Scenario: Create a new study group with occupied subject
   Given I create a 'new' study group with 'Math' subject
	When I create a 'new' study group with 'Math' subject
	Then 'Conflict' status is returned

Scenario: Create a new study group with empty name and subject
	When I create a '' study group with '' subject
	Then 'BadRequest' status is returned

Scenario: When there is no data in database list of study groups should be empty
	When I ask for a list of study groups
	Then the list of study groups 'is empty'

Scenario: When there is data in database list of study groups should have this data
   Given I create a 'new' study group with 'Math' subject
	When I ask for a list of study groups
	Then the list of study groups 'contains new group'

Scenario: Search is resolved with no data when there is no study group
	When I search for a list of study groups by 'Chemistry' subject
	Then the list of study groups 'is empty'

Scenario: Search is resolved with no data when there is no study group with specified subject
   Given I create a 'new' study group with 'Math' subject
	When I search for a list of study groups by 'Chemistry' subject
	Then the list of study groups 'is empty'

Scenario: I'm able to search for a new study group
   Given I create a 'new' study group with 'Math' subject
	When I search for a list of study groups by 'Math' subject
	Then the list of study groups 'contains new group'

Scenario: Search is resolved only with data corresponging to specified subject
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' study group with 'Physics' subject
	When I search for a list of study groups by 'Physics' subject
	Then the list of study groups 'contains new group'

Scenario: Join should not be possible with definitely invalid data
	When I 'join' a '' study group as '' user
	Then 'BadRequest' status is returned

Scenario: Join to non-existing study group
   Given I create a 'new' user
	When I 'join' a 'non-existing' study group as 'new' user
	Then 'NotFound' status is returned

Scenario: Join as non-existing user
   Given I create a 'new' study group with 'Chemistry' subject
	When I 'join' a 'new' study group as 'non-existing' user
	Then 'NotFound' status is returned

Scenario: User should be able to join the study group
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' user
	When I 'join' a 'new' study group as 'new' user
	Then 'OK' status is returned

Scenario: User should not be able to join the study group twice
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' user
	When I 'join' a 'new' study group as 'new' user
	 And I 'join' a 'new' study group as 'new' user
	Then 'Conflict' status is returned

Scenario: User should be able to join several groups
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' user
	 And I 'join' a 'new' study group as 'new' user
	 And I create a 'new' study group with 'Physics' subject
	When I 'join' a 'new' study group as 'new' user
	Then 'OK' status is returned

Scenario: Leave should not be possible with definitely invalid data
	When I 'leave' a '' study group as '' user
	Then 'BadRequest' status is returned

Scenario: Leave non-existing study group
   Given I create a 'new' user
	When I 'leave' a 'non-existing' study group as 'new' user
	Then 'NotFound' status is returned

Scenario: Leave as non-existing user
   Given I create a 'new' study group with 'Chemistry' subject
	When I 'leave' a 'new' study group as 'non-existing' user
	Then 'NotFound' status is returned

Scenario: Leave not assigned study group
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' user
	When I 'leave' a 'new' study group as 'new' user
	Then 'NotFound' status is returned

Scenario: User should be able to leave the assigned study group
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' user
	 And I 'join' a 'new' study group as 'new' user
	When I 'leave' a 'new' study group as 'new' user
	Then 'OK' status is returned

Scenario: User should be able to re-join the study group
   Given I create a 'new' study group with 'Chemistry' subject
     And I create a 'new' user
	 And I 'join' a 'new' study group as 'new' user
	 And I 'leave' a 'new' study group as 'new' user
	When I 'join' a 'new' study group as 'new' user
	Then 'OK' status is returned
	 And I ask for a list of study groups
	 And the list of study groups 'contains new group'
	 And the 'new' study group contains 'new' user
```


<details>
<summary>StudyGroupStepDefinitions.cs</summary>

```csharp
namespace EPAM.StudyGroups.Tests.SpecFlow.StepDefinitions
{
    [Binding]
    public sealed class StudyGroupStepDefinitions
    {
        private const string TestClient = "TEST_CLIENT";
        private const string NewStudyGroupName = "NEW_STUDY_GROUP_NAME";
        private const string NewStudyGroupSubject = "NEW_STUDY_GROUP_SUBJECT";
        private const string LastResponse = "LAST_RESPONSE";
        private const string LastData = "LAST_DATA";
        private const string LastUser = "LAST_USER";

        private static readonly string apiConnectionString;
        private static readonly string dbConnectionString;

        public static readonly string CurrentEnvironemnt = EnvironmentVariables.TestEnvironment ?? TestEnvironments.Development;

        private readonly ScenarioContext scenarioContext;

        static StudyGroupStepDefinitions()
        {
            IConfigurationRoot config =
                new ConfigurationBuilder()
                    .AddJsonFile("launchSettings.json")
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{CurrentEnvironemnt}.json", true)
                    .Build();

            string connectionStrings = config.GetValue<string>("profiles:EPAM.StudyGroups.Api:applicationUrl");
            apiConnectionString = connectionStrings.Split(';')[0];

            dbConnectionString = config.GetValue<string>("ConnectionStrings:StudyGroupsContext");
        }

        public StudyGroupStepDefinitions(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            StudyGroupClient client = 
                new StudyGroupPersistenceClient(
                    new StudyGroupClient(
                        new HttpClient
                        {
                            BaseAddress = new Uri(apiConnectionString),
                        }),
                    dbConnectionString);

            this.scenarioContext[TestClient] = client;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            (this.scenarioContext[TestClient] as StudyGroupClient)?.Dispose();
        }

        [Given("I create a '(.*)' study group with '(.*)' subject")]
        [When("I create a '(.*)' study group with '(.*)' subject")]
        public async Task CreateNewStudyGroup(string groupNameType, string subject)
        {
            var client = this.scenarioContext[TestClient] as StudyGroupClient;
            HttpResponseMessage response;

            string newStudyGroupName = groupNameType;
            string newStudyGroupSubject = subject;

            switch (groupNameType.ToLower())
            {
                case "new":
                    newStudyGroupName = BaseControllerTests.GetRandomName();

                    this.scenarioContext[NewStudyGroupName] = newStudyGroupName;
                    this.scenarioContext[NewStudyGroupSubject] = newStudyGroupSubject;

                    break;
                case "existing":
                    newStudyGroupName = this.scenarioContext[NewStudyGroupName] as string;

                    break;

                case "":

                    break;
                default:
                    throw new NotImplementedException($"Creation of '{groupNameType}' study group has not been implemented yet.");
            }

            response = await client
                .TryCreateStudyGroupAsync(
                    new CreateStudyGroupRequest()
                    {
                        Name = newStudyGroupName,
                        Subject = newStudyGroupSubject,
                    })
                .ConfigureAwait(false);

            this.scenarioContext[LastResponse] = response;
        }

        [Given("I create a '(.*)' user")]
        [When("I create a '(.*)' user")]
        public async Task I_create_a_user(string userType)
        {
            User user = null;

            switch (userType.ToLower())
            {
                case "new":
                    using (StudyGroupsContext context = this.GetContext())
                    {
                        user = new User
                        {
                            Email = $"{BaseControllerTests.GetRandomName()}@test.com",
                            FirstName = BaseControllerTests.GetRandomName(),
                            LastName = BaseControllerTests.GetRandomName(),
                        };

                        user = (await context
                            .Users
                            .AddAsync(user)
                            .ConfigureAwait(false))
                            .Entity;

                        await context.SaveChangesAsync().ConfigureAwait(false);
                    }

                    break;
                default:
                    throw new NotImplementedException($"Creation of '{userType}' user has not been implemented yet.");
            }

            this.scenarioContext[LastUser] = user;
        }

        [When("I ask for a list of study groups")]
        [Then("I ask for a list of study groups")]
        public async Task I_ask_for_a_list_of_study_groups()
        {
            var client = this.scenarioContext[TestClient] as StudyGroupClient;

            (TestStudyGroup[] data, HttpResponseMessage response) = await client
                .TryGetStudyGroupsAsync()
                .ConfigureAwait(false);

            this.scenarioContext[LastData] = data;
            this.scenarioContext[LastResponse] = response;
        }

        [When("I search for a list of study groups by '(.*)' subject")]
        public async Task I_search_for_a_list_of_study_groups(string subject)
        {
            var client = this.scenarioContext[TestClient] as StudyGroupClient;

            (StudyGroup[] data, HttpResponseMessage response) = await client
                .TrySearchStudyGroupsAsync(subject)
                .ConfigureAwait(false);

            this.scenarioContext[LastData] = data;
            this.scenarioContext[LastResponse] = response;
        }

        [Given("I '(.*)' a '(.*)' study group as '(.*)' user")]
        [When("I '(.*)' a '(.*)' study group as '(.*)' user")]
        public async Task I_join_to_a_study_group_as(string action, string studyGroup, string user)
        {
            var client = this.scenarioContext[TestClient] as StudyGroupClient;

            string userId = null;
            string studyGroupId = null;

            switch (user)
            {
                case "new":
                    userId = (this.scenarioContext[LastUser] as User).Id.ToString();
                    break;
                case "non-existing":
                    userId = int.MaxValue.ToString();
                    break;
                case "":
                    userId = string.Empty;
                    break;
            }

            switch (studyGroup)
            {
                case "new":
                    studyGroupId = (this.scenarioContext[NewStudyGroupName] as string);

                    using (StudyGroupsContext context = GetContext())
                    {
                        studyGroupId = (await context
                            .StudyGroups
                            .FirstOrDefaultAsync(g => g.Name == studyGroupId)
                            .ConfigureAwait(false))
                            .StudyGroupId
                            .ToString();
                    }

                    break;
                case "":
                    studyGroupId = string.Empty;
                    break;
                case "non-existing":
                    studyGroupId = int.MaxValue.ToString();
                    break;
            }

            switch (action.ToLower())
            {
                case "join":
                    this.scenarioContext[LastResponse] =
                        await client
                            .TryJoinStudyGroupAsync(
                                studyGroupId: studyGroupId,
                                userId: userId)
                            .ConfigureAwait(false);
                    break;
                case "leave":
                    this.scenarioContext[LastResponse] =
                        await client
                            .TryLeaveStudyGroupAsync(
                                studyGroupId: studyGroupId,
                                userId: userId)
                            .ConfigureAwait(false);
                    break;
                default:
                    throw new NotImplementedException($"Action '{action}' has not been implemented yet.");
            }
        }

        [Then("'(.*)' study group with '(.*)' subject has been created")]
        public async Task ThenTheResultShouldBe(string groupNameType, string expectedSubject)
        {
            switch (groupNameType.ToLower())
            {
                case "new":
                    var client = this.scenarioContext[TestClient] as StudyGroupClient;

                    var result = await client
                        .GetStudyGroupsAsync()
                        .ConfigureAwait(false);

                    result.Should()
                        .HaveCount(1)
                        .And
                        .Subject
                        .First()
                        .Should()
                        .BeEquivalentTo(
                            new StudyGroup
                            {
                                Name = this.scenarioContext[NewStudyGroupName] as string,
                                Subject = 
                                    Enum.Parse<Subject>(expectedSubject),
                                CreateDate = DateTime.UtcNow,
                            },
                            config => config
                                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 15.Seconds()))
                                .WhenTypeIs<DateTime>()
                                .Excluding(o => o.Users)
                                .Excluding(o => o.StudyGroupId))
                        .And
                        .Subject
                        .As<StudyGroup>()
                        .StudyGroupId
                        .Should()
                        .BeGreaterThan(0);
                    break;
                default:
                    throw new PendingStepException();
            }
        }

        [Then("'(.*)' status is returned")]
        public void ThenTheResultShouldBe(string responseType)
        {
            var client = this.scenarioContext[LastResponse] as HttpResponseMessage;

            switch (responseType.ToLower())
            {
                case "conflict":
                    client.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
                    break;
                case "badrequest":
                    client.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
                    break;
                case "notfound":
                    client.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
                    break;
                case "ok":
                    client.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                    break;
                default:
                    throw new NotImplementedException($"Processing of '{responseType}' response has not been implemented yet.");
            }
        }

        [Then("the list of study groups '(.*)'")]
        public void The_list_of_study_groups(string stateOption)
        {
            var data = this.scenarioContext[LastData] as StudyGroup[];
            var response = this.scenarioContext[LastResponse] as HttpResponseMessage;

            switch (stateOption.ToLower())
            {
                case "is empty":
                    response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                    data.Should().BeEmpty();
                    break;
                case "contains new group":
                    response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

                    data.Should()
                        .HaveCount(1)
                        .And
                        .Subject
                        .First()
                        .Should()
                        .BeEquivalentTo(
                            new StudyGroup
                            {
                                Name = this.scenarioContext[NewStudyGroupName] as string,
                                Subject = Enum.Parse<Subject>(this.scenarioContext[NewStudyGroupSubject] as string),
                                CreateDate = DateTime.UtcNow,
                            },
                            config => config
                                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 15.Seconds()))
                                .WhenTypeIs<DateTime>()
                                .Excluding(o => o.Users)
                                .Excluding(o => o.StudyGroupId))
                        .And
                        .Subject
                        .As<StudyGroup>()
                        .StudyGroupId
                        .Should()
                        .BeGreaterThan(0);
                    break;
                default:
                    throw new NotImplementedException($"'{stateOption}' state of study groups' list has not been implemented yet.");
            }
        }

        [Then("the '(.*)' study group contains '(.*)' user")]
        public void The_study_group_contains_user(string studyGroup, string userType)
        {
            var data = this.scenarioContext[LastData] as TestStudyGroup[];
            var response = this.scenarioContext[LastResponse] as HttpResponseMessage;

            string studyGroupName = null;
            User user = null;

            switch (studyGroup)
            {
                case "new":
                    studyGroupName = this.scenarioContext[NewStudyGroupName] as string;
                    break;
                default:
                    throw new PendingStepException();
            }

            switch (userType)
            {
                case "new":
                    user = this.scenarioContext[LastUser] as User;
                    break;
                default:
                    throw new PendingStepException();
            }

            data
                .Single(g => g.Name == studyGroupName)
                .Users
                .Should()
                .NotBeEmpty()
                .And
                .ContainEquivalentOf(user, config => config.Excluding(o => o.StudyGroups));
        }

        private StudyGroupsContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<StudyGroupsContext>();
            optionsBuilder.UseSqlServer(dbConnectionString);

            return new StudyGroupsContext(optionsBuilder.Options);
        }
    }
}
```
</details>

## NFR and Load/Performance testing

```csharp
namespace EPAM.StudyGroups.Tests.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfigurationRoot config =
                new ConfigurationBuilder()
                    .AddJsonFile("launchSettings.json")
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                    .Build();

            string connectionStrings = config.GetValue<string>("profiles:EPAM.StudyGroups.Api:applicationUrl");
            string apiConnectionString = connectionStrings.Split(';')[0];

            string dbConnectionString = config.GetValue<string>("ConnectionStrings:StudyGroupsContext");

            using StudyGroupClient client = new StudyGroupClient(new HttpClient { BaseAddress = new Uri(apiConnectionString) });

            var scenario = Scenario.Create("http_scenario", async context =>
            {
                await Step.Run("Get study groups", context, async () => 
                {
                    (TestStudyGroup[] data, HttpResponseMessage response) = 
                        await client.TryGetStudyGroupsAsync().ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        return Response.Ok(payload: data, response.StatusCode.ToString());
                    }
                    return Response.Fail<TestStudyGroup[]>(statusCode: response.StatusCode.ToString());
                });
                await Step.Run("Search study groups", context, async () =>
                {
                    (StudyGroup[] data, HttpResponseMessage response) =
                        await client.TrySearchStudyGroupsAsync("Math").ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        return Response.Ok(payload: data, response.StatusCode.ToString());
                    }
                    return Response.Fail<StudyGroup[]>(statusCode: response.StatusCode.ToString());
                });
                return Response.Ok();
            })
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.Inject(rate: 100,
                                  interval: TimeSpan.FromSeconds(1),
                                  during: TimeSpan.FromSeconds(30))
            );

            NBomberRunner
                .RegisterScenarios(scenario)
                .Run();
        }
    }
}
```
