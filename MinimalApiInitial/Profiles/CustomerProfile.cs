using AutoMapper;
using MinimalApiInitial.Dtos;
using MinimalApiInitial.Entities;

namespace MinimalApiInitial.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<CustomerDto, Customer>();
    }
}