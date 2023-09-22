using Entities;

namespace Repositories.BaseRepository;

public interface IBaseRepository<T, TKey> where T : BaseEntity
{
    void Update(T entity);
    void Create(T entity);
    void Delete(T entity);
}
