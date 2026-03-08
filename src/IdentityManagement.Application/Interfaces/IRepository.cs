using System.Linq.Expressions;
using IdentityManagement.Application.Common;
using IdentityManagement.Domain.Interfaces;

namespace IdentityManagement.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<PagedResult<T>> GetPagedAsync(
        Expression<Func<T, bool>>? filter,
        PagedRequest request,
        IEnumerable<string>? searchPropertyNames = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetListAsync(
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default);
}
