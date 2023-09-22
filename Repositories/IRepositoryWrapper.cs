using Repositories.UserRepository;

namespace Repositories;

public interface IRepositoryWrapper
{
    IUserRepository Users { get; }
    Task SaveAsync();
}
