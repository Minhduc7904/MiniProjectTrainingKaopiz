using Entities;
using Repositories.BaseRepository;

namespace Repositories.UserRepository;

public class UserRepository : BaseRepository<User, int>, IUserRepository
{
    public UserRepository(MySqlDbContext context) : base(context)
    {
    }
}
