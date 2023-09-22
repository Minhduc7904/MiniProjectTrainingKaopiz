using Entities;
using Repositories.UserRepository;

namespace Repositories;

public class RepositoryWrapper : IRepositoryWrapper
{
    private readonly MySqlDbContext _context;
    private IUserRepository _user;

    public RepositoryWrapper(MySqlDbContext context) => _context = context;


    public IUserRepository Users
    {
        get
        {
            if (_user == null) _user = new UserRepository.UserRepository(_context);
            return _user;
        }
    }

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();

}
