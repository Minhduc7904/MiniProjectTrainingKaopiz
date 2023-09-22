using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Constant;
using Dtos.AccountDto;
using Microsoft.IdentityModel.Tokens;
using Repositories;

namespace Services.AccountService;

//TODO: THIS
public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repository;

    public UserService(IMapper mapper, IRepositoryWrapper repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<LoginResponseDto> Login()
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345"));
        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var tokeOptions = new JwtSecurityToken(
            claims: new List<Claim>
            {
                new("id", 1 + ""),
                new("user-role", UserRole.MANAGER.ToString())
            },
            expires: DateTime.Now.AddMinutes(5),
            signingCredentials: signinCredentials
        );
        return new LoginResponseDto
        {
            Id = 1,
            FullName = "test test",
            Role = UserRole.MANAGER,
            Token = new JwtSecurityTokenHandler().WriteToken(tokeOptions)
        };
    }

    public Task<int> Signup()
    {
        throw new NotImplementedException();
    }
}
