using AutoMapper;
using Dtos.AccountDto;
using Entities;

namespace Utils.Mappers;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<UserInfo, SignupRequestDto>().ReverseMap();
        CreateMap<UserInfo, AccountInfoFullDto>().ReverseMap();
    }
}
