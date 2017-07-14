using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Services;
using JoyOI.ManagementService.WebApi.WebApiModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class ActorController : Controller
    {
        private IActorService _actorService;

        public ActorController(IActorService actorService)
        {
            _actorService = actorService;
        }

        [HttpGet("All")]
        public async Task<ApiResponse<IList<ActorOutputDto>>> Get()
        {
            var dtos = await _actorService.GetAll(null);
            return ApiResponse.OK(dtos);
        }

        [HttpGet("{name}")]
        public async Task<ApiResponse<ActorOutputDto>> Get(string name)
        {
            var dto = await _actorService.Get(name);
            if (dto == null)
                return ApiResponse.NotFound("actor not found", dto);
            return ApiResponse.OK(dto);
        }

        [HttpPut]
        public async Task<ApiResponse<PutResult<Guid>>> Put([FromBody]ActorInputDto dto)
        {
            var key = await _actorService.Put(dto);
            var result = new PutResult<Guid>(key);
            return ApiResponse.OK(result);
        }

        [HttpPatch("{name}")]
        public async Task<ApiResponse<PatchResult>> Patch(string name, [FromBody]ActorInputDto dto)
        {
            var patched = await _actorService.Patch(name, dto);
            var result = new PatchResult(patched);
            if (patched <= 0)
                return ApiResponse.NotFound("actor not found", result);
            return ApiResponse.OK(result);
        }

        [HttpDelete("{name}")]
        public async Task<ApiResponse<DeleteResult>> Delete(string name)
        {
            var deleted = await _actorService.Delete(name);
            var result = new DeleteResult(deleted);
            if (deleted <= 0)
                return ApiResponse.NotFound("actor not found", result);
            return ApiResponse.OK(result);
        }
    }
}
