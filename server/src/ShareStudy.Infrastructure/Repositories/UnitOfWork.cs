using System.Collections.Concurrent;
using ShareStudy.Application.Interfaces.Repositories;
using ShareStudy.Infrastructure.Persistence;

namespace ShareStudy.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ConcurrentDictionary<string, object> _repositories;
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _repositories = new ConcurrentDictionary<string, object>();
    }

    public IRepository<T> Repository<T>() where T : class
    {
        var typeName = typeof(T).Name;

        return (IRepository<T>)_repositories.GetOrAdd(typeName, _ => new Repository<T>(_dbContext));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _dbContext.Dispose();
        }
        _disposed = true;
    }
}
