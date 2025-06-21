using Microsoft.EntityFrameworkCore.Storage;
using Gideon.WPF.Core.Domain.Interfaces;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Infrastructure.Data.Context;

namespace Gideon.WPF.Infrastructure.Data.Repositories;

/// <summary>
/// Unit of Work implementation for managing database transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly GideonDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    // Repository instances
    private IRepository<Character>? _characters;
    private IRepository<Ship>? _ships;
    private IRepository<Module>? _modules;
    private IRepository<ShipFitting>? _shipFittings;

    public UnitOfWork(GideonDbContext context)
    {
        _context = context;
    }

    // Repository properties
    public IRepository<Character> Characters
    {
        get
        {
            _characters ??= new Repository<Character>(_context);
            return _characters;
        }
    }

    public IRepository<Ship> Ships
    {
        get
        {
            _ships ??= new Repository<Ship>(_context);
            return _ships;
        }
    }

    public IRepository<Module> Modules
    {
        get
        {
            _modules ??= new Repository<Module>(_context);
            return _modules;
        }
    }

    public IRepository<ShipFitting> ShipFittings
    {
        get
        {
            _shipFittings ??= new Repository<ShipFitting>(_context);
            return _shipFittings;
        }
    }

    // Transaction management
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction started");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction started");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // Save changes
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _context.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    // Generic repository access
    public IRepository<T> Repository<T>() where T : class
    {
        return new Repository<T>(_context);
    }

    // Disposal
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
            _disposed = true;
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }

        await _context.DisposeAsync();
        _disposed = true;
    }
}