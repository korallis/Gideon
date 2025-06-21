using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing repository transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repository properties
    IRepository<Character> Characters { get; }
    
    IRepository<Ship> Ships { get; }
    
    IRepository<Module> Modules { get; }
    
    IRepository<ShipFitting> ShipFittings { get; }
    
    // Transaction management
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    // Bulk operations
    Task<int> ExecuteSqlAsync(string sql, params object[] parameters);
    
    Task<int> ExecuteSqlAsync(string sql, CancellationToken cancellationToken, params object[] parameters);
}