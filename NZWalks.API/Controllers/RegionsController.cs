using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.OpenApi.Any;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;

namespace NZWalks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionsController : ControllerBase
    {
        private readonly NZWalksDbContext dbContext;

        public RegionsController(NZWalksDbContext dbContext)
        {
            this.dbContext = dbContext; 
        }

        //GET ALL REGIONS
        [HttpGet]
        public IActionResult GetAll()
        {
            //Get Data From Database - Domain models
           var regionsDomain = dbContext.Regions.ToList();

            //Map Domain Models to DTOs
            var regionsDto = new List<RegionDto>();
            foreach(var regionDomain in regionsDomain)
            {
                regionsDto.Add(new RegionDto()
                {
                    Id = regionDomain.Id,
                    Code = regionDomain.Code,
                    Name = regionDomain.Name,
                    RegionImage = regionDomain.RegionImage
                });           
            }
            //Return DTOs
            return Ok(regionsDto);
        }

        //GET SINGLE REGION (Get Region By ID)

        [HttpGet]
        [Route("{id:Guid}")]
        public IActionResult GetById([FromRoute]Guid id)
        {
            //Get Region Domain Model From DataBase
            var regionDomain = dbContext.Regions.FirstOrDefault(x => x.Id == id);

            if(regionDomain == null)
            {
                return NotFound();
            }

            //Map/Convert Region Domain Model to Region Dto
            var regionsDto = new RegionDto
            {
                Id = regionDomain.Id,
                Code = regionDomain.Code,
                Name = regionDomain.Name,
                RegionImage = regionDomain.RegionImage
            };
            return Ok(regionsDto);
         
        }

        //Post To Create New Region
        [HttpPost]
        public IActionResult Create([FromBody] AddRegionRequestDto addRegionRequestDto)
        {
            //Map or Convert DTO to Domain Model
            var regionDomainModel = new Region
            {
                Code = addRegionRequestDto.Code,
                Name = addRegionRequestDto.Name,
                RegionImage = addRegionRequestDto.RegionImage
            };

            //Use Domain Model to Create Region
            dbContext.Regions.Add(regionDomainModel);
            dbContext.SaveChanges();

            //Map Domain model back to DTO
            var regionsDto = new RegionDto()
            {
                Id = regionDomainModel.Id,
                Code = regionDomainModel.Code,
                Name = regionDomainModel.Name,
                RegionImage = regionDomainModel.RegionImage
            };

            return CreatedAtAction(nameof(GetById), new { id = regionsDto.Id }, regionsDto);
        }

        [HttpPut]
        public IActionResult Update(Guid id, Region region)
        {
            if(id != region.Id)
            {
                return BadRequest();
            }
            dbContext.Entry(region).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            try
            {
                dbContext.SaveChanges();
            }
            catch(DbUpdateConcurrencyException)
            {
                if (!RegionAvailable(id))
                {
                    return NotFound();
                }
                else
                {
                    return Conflict();
                }
            }
            return Ok();
        }

        private bool RegionAvailable(Guid id)
        {
            return dbContext.Regions.Any(x => x.Id == id);
        }



        [HttpDelete]
        public IActionResult DeleteById(Guid id)
        {          
            if (dbContext.Regions == null)
            {
                return NotFound();
            }

            var regionDomainModel = dbContext.Regions.Find(id);

            dbContext.Regions.Remove(regionDomainModel);

            dbContext.SaveChanges();

            return Ok();
        }
    }
}
