using AutoMapper;
using MinimalApiFull.Dtos;
using MinimalApiFull.Entities;

namespace MinimalApiFull.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<CustomerDto, Customer>();
    }
}