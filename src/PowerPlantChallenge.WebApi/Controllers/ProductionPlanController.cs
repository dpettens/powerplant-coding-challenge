using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PowerPlantChallenge.WebApi.DTOs;
using PowerPlantChallenge.WebApi.Models;
using PowerPlantChallenge.WebApi.Services;

namespace PowerPlantChallenge.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductionPlanController : ControllerBase
    {
        private readonly IProductionPlanService _productionPlanService;
        private readonly IMapper _mapper;

        public ProductionPlanController(IProductionPlanService productionPlanService, IMapper mapper)
        {
            _productionPlanService = productionPlanService ?? throw new ArgumentNullException(nameof(productionPlanService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<PowerPlanLoadDto>> Calculate([FromBody] ProductionPlanDto productionPlanDto)
        {
            var productionPlan = _mapper.Map<ProductionPlan>(productionPlanDto);
            var powerPlantLoads = _productionPlanService.CalculateUnitCommitment(productionPlan);
            
            return Ok(_mapper.Map<IEnumerable<PowerPlanLoadDto>>(powerPlantLoads));
        }
    }
}