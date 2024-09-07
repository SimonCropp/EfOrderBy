[TestFixture]
[Parallelizable(ParallelScope.All)]
public class CoreTests
{
    [Test]
    public async Task MissingOrderBy()
    {
        await using var database = await DbContextBuilder.GetOrderRequiredDatabase();
        var data = database.Context;
        await ThrowsTask(
                () => data.Companies
                    .ToListAsync())
            .IgnoreStackTrace();
    }

    [Test]
    public async Task NestedMissingOrderBy()
    {
        await using var database = await DbContextBuilder.GetOrderRequiredDatabase();
        var data = database.Context;
        await ThrowsTask(
                () => data.Companies
                    .Include(_ => _.Employees)
                    .OrderBy(_ => _.Name)
                    .ToListAsync())
            .IgnoreStackTrace();
    }

    [Test]
    public async Task WithOrderBy()
    {
        await using var database = await DbContextBuilder.GetOrderRequiredDatabase();
        var data = database.Context;
        await Verify(
            data.Companies
                .OrderBy(_ => _.Name)
                .ToListAsync());
    }

    [Test]
    public async Task SingleMissingOrder()
    {
        await using var database = await DbContextBuilder.GetOrderRequiredDatabase();
        var data = database.Context;
        await Verify(data.Companies.Where(_ => _.Name == "Company1").SingleAsync());
    }

    [Test]
    public async Task WithNestedOrderBy()
    {
        await using var database = await DbContextBuilder.GetOrderRequiredDatabase();
        var data = database.Context;
        await Verify(
            data.Companies
                .Include(_ => _.Employees.OrderBy(_ => _.Age))
                .OrderBy(_ => _.Name)
                .ToListAsync());
    }


    #region IgnoreNavigationProperties

    [Test]
    public async Task IgnoreNavigationProperties()
    {
        var options = DbContextOptions();

        await using var data = new SampleDbContext(options);

        var company = new Company
        {
            Name = "company"
        };
        var employee = new Employee
        {
            Name = "employee",
            Company = company
        };
        await Verify(employee)
            .IgnoreNavigationProperties();
    }

    #endregion

    #region IgnoreNavigationPropertiesExplicit

    [Test]
    public async Task IgnoreNavigationPropertiesExplicit()
    {
        var options = DbContextOptions();

        await using var data = new SampleDbContext(options);

        var company = new Company
        {
            Name = "company"
        };
        var employee = new Employee
        {
            Name = "employee",
            Company = company
        };
        await Verify(employee)
            .IgnoreNavigationProperties(data);
    }

    #endregion

    static void IgnoreNavigationPropertiesGlobal()
    {
        #region IgnoreNavigationPropertiesGlobal

        var options = DbContextOptions();
        using var data = new SampleDbContext(options);
        VerifyEntityFramework.IgnoreNavigationProperties();

        #endregion
    }

    static void IgnoreNavigationPropertiesGlobalExplicit()
    {
        #region IgnoreNavigationPropertiesGlobalExplicit

        var options = DbContextOptions();
        using var data = new SampleDbContext(options);
        VerifyEntityFramework.IgnoreNavigationProperties(data.Model);

        #endregion
    }

    [Test]
    public async Task WithNavigationProp()
    {
        var options = DbContextOptions();

        await using var data = new SampleDbContext(options);
        var company = new Company
        {
            Name = "companyBefore"
        };
        data.Add(company);
        var employee = new Employee
        {
            Name = "employeeBefore",
            Company = company
        };
        data.Add(employee);
        await data.SaveChangesAsync();

        data.Companies.Single()
            .Name = "companyAfter";
        data.Employees.Single()
            .Name = "employeeAfter";
        await Verify(data.ChangeTracker);
    }

    [Test]
    public async Task SomePropsModified()
    {
        var options = DbContextOptions();

        await using var data = new SampleDbContext(options);
        var employee = new Employee
        {
            Name = "before",
            Age = 10
        };
        data.Add(employee);
        await data.SaveChangesAsync();

        data.Employees.Single()
            .Name = "after";
        await Verify(data.ChangeTracker);
    }

    [Test]
    public Task ShouldIgnoreDbFactory() =>
        Verify(new MyDbContextFactory());

    [Test]
    public Task ShouldIgnoreDbFactoryInterface()
    {
        var target = new TargetWithFactoryInterface
        {
            Factory = new MyDbContextFactory()
        };
        return Verify(target);
    }

    class TargetWithFactoryInterface
    {
        public IDbContextFactory<SampleDbContext> Factory { get; set; } = null!;
    }

    [Test]
    public Task ShouldIgnoreDbContext() =>
        Verify(
            new
            {
                Factory = new SampleDbContext(new DbContextOptions<SampleDbContext>())
            });

    class MyDbContextFactory : IDbContextFactory<SampleDbContext>
    {
        public SampleDbContext CreateDbContext() =>
            throw new NotImplementedException();
    }

    [Test]
    public async Task UpdateEntity()
    {
        var options = DbContextOptions();

        await using var data = new SampleDbContext(options);
        data.Add(new Employee
        {
            Name = "before"
        });
        await data.SaveChangesAsync();

        var employee = data.Employees.Single();
        data.Update(employee)
            .Entity.Name = "after";
        await Verify(data.ChangeTracker);
    }

    [Test]
    public async Task AllData()
    {
        var database = await DbContextBuilder.GetDatabase();
        var data = database.Context;

        #region AllData

        await Verify(data.AllData())
            .AddExtraSettings(
                serializer =>
                    serializer.TypeNameHandling = TypeNameHandling.Objects);

        #endregion
    }

    [Test]
    public async Task NestedQueryable()
    {
        var database = await DbContextBuilder.GetDatabase();
        await database.AddData(
            new Company
            {
                Name = "value"
            });
        var data = database.Context;
        var queryable = data.Companies
            .Where(_ => _.Name == "value");
        await Verify(
            new
            {
                queryable
            });
    }


    static DbContextOptions<SampleDbContext> DbContextOptions(
        [CallerMemberName] string databaseName = "") =>
        new DbContextOptionsBuilder<SampleDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
}