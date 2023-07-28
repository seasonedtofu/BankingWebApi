using BankingWebApi.Models;
using BankingWebApi.DataTransformationObjects;
using AutoMapper;

namespace BankingWebApi.Profiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Account, AccountDto>();
        }
    }
}
