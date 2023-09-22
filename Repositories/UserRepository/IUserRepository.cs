using Entities;
using Repositories.BaseRepository;

namespace Repositories.UserRepository;

public interface IUserRepository : IBaseRepository<User, int>
{
}
