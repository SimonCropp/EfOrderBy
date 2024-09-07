namespace EfOrderBy;

public static class EfOrderByExtensions
{
    public static DbContextOptionsBuilder<TContext> ThrowForMissingOrderBy<TContext>(this DbContextOptionsBuilder<TContext> builder)
        where TContext : DbContext =>
        builder.ReplaceService<IShapedQueryCompilingExpressionVisitorFactory, RelationalFactory>();

}