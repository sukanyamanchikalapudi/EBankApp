using AutoMapper;
using EBankApp.Models;

namespace EBankApp.AutoMapperConfig
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UpdateProfileRequest, User>();
            CreateMap<User, UpdateProfileRequest>();
        }
    }
}