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
            var dtos = await _actorService.Get();
            return ApiResponse.OK(dtos);
        }

        [HttpGet]
        public async Task<ApiResponse<ActorOutputDto>> Get([FromQuery]Guid id)
        {
            var dto = await _actorService.Get(id);
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

        [HttpPatch]
        public async Task<ApiResponse<PatchResult>> Patch([FromQuery] Guid id, [FromBody]ActorInputDto dto)
        {
            var patched = await _actorService.Patch(id, dto);
            var result = new PatchResult(patched);
            if (dto == null)
                return ApiResponse.NotFound("actor not found", result);
            return ApiResponse.OK(result);
        }

        [HttpDelete]
        public async Task<ApiResponse<DeleteResult>> Delete([FromQuery] Guid id)
        {
            var deleted = await _actorService.Delete(id);
            var result = new DeleteResult(deleted);
            if (!deleted)
                return ApiResponse.NotFound("actor not found", result);
            return ApiResponse.OK(result);
        }
    }
}
