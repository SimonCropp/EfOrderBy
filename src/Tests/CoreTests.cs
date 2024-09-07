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
}