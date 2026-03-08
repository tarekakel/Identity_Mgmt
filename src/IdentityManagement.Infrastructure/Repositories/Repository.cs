using System.Linq.Expressions;
using System.Reflection;
using IdentityManagement.Application.Common;
using IdentityManagement.Application.Interfaces;
using IdentityManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityManagement.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return entity != null;
    }

    public virtual void Add(T entity)
    {
        DbSet.Add(entity);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        DbSet.Remove(entity);
    }

    public virtual async Task<IReadOnlyList<T>> GetListAsync(
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = DbSet.AsQueryable();
        if (filter != null)
            query = query.Where(filter);
        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(
        Expression<Func<T, bool>>? filter,
        PagedRequest request,
        IEnumerable<string>? searchPropertyNames = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = DbSet.AsQueryable();

        if (filter != null)
            query = query.Where(filter);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm) && searchPropertyNames != null)
            query = ApplySearch(query, request.SearchTerm.Trim(), searchPropertyNames);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySort(query, request.SortBy, request.SortDescending);

        var pageNumber = request.GetPageNumber();
        var pageSize = request.GetPageSize();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    private static IQueryable<T> ApplySearch(IQueryable<T> query, string searchTerm, IEnumerable<string> propertyNames)
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        Expression? combined = null;
        var searchLower = searchTerm.ToLower();
        var searchConstant = Expression.Constant(searchLower);

        foreach (var propName in propertyNames)
        {
            var prop = typeof(T).GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null || prop.PropertyType != typeof(string))
                continue;

            var propAccess = Expression.Property(parameter, prop);
            var toLower = Expression.Call(propAccess, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
            var containsCall = Expression.Call(toLower, containsMethod, searchConstant);
            combined = combined == null ? containsCall : Expression.OrElse(combined, containsCall);
        }

        if (combined == null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
        return query.Where(lambda);
    }

    private static IQueryable<T> ApplySort(IQueryable<T> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        var prop = typeof(T).GetProperty(sortBy, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop == null)
            return query;

        var parameter = Expression.Parameter(typeof(T), "e");
        var propAccess = Expression.Property(parameter, prop);
        var lambda = Expression.Lambda(propAccess, parameter);
        var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), prop.PropertyType },
            query.Expression,
            Expression.Quote(lambda));
        return query.Provider.CreateQuery<T>(resultExpression);
    }
}
