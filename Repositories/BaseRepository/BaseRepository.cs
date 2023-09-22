using Entities;

namespace Repositories.BaseRepository;

public abstract class BaseRepository<T, TKey> : IBaseRepository<T, TKey> where T : BaseEntity
{
    private readonly MySqlDbContext _context;

    protected BaseRepository(MySqlDbContext context)
    {
        _context = context;
    }

    public void Create(T entity) => _context.Set<T>().Add(entity);

    public void Update(T entity) => _context.Set<T>().Update(entity);

    public void Delete(T entity)
    {
        entity.DeletedAt = new DateTime();
        _context.Set<T>().Update(entity);
    }
}
