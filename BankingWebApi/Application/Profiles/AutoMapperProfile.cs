using BankingWebApi.Domain.Entities;
using BankingWebApi.Application.Models;
using AutoMapper;

namespace BankingWebApi.Application.Profiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Account, AccountDto>();
        }
    }
}
