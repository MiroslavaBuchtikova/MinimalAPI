using AutoMapper;
using MinimalApiNalejvarna.Dtos;
using MinimalApiNalejvarna.Entities;

namespace MinimalApiNalejvarna.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<CustomerDto, Customer>();
    }
}