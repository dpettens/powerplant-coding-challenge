using AutoMapper;
using PowerPlantChallenge.WebApi.DTOs;
using PowerPlantChallenge.WebApi.Models;

namespace PowerPlantChallenge.WebApi.Profiles
{
    public class ProductionPlanProfile : Profile
    {
        public ProductionPlanProfile()
        {
            CreateMap<ProductionPlanDto, ProductionPlan>();
            CreateMap<FuelsDto, Fuels>();
            CreateMap<PowerPlantDto, PowerPlant>();
            CreateMap<PowerPlantLoad, PowerPlanLoadDto>();
        }
    }
}