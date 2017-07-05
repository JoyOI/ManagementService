using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Services;
using JoyOI.ManagementService.WebApi.WebApiModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class StateMachineController : Controller
    {
        private IStateMachineService _stateMachineService;

        public StateMachineController(IStateMachineService stateMachineService)
        {
            _stateMachineService = stateMachineService;
        }

        [HttpGet("All")]
        public async Task<ApiResponse<IList<StateMachineOutputDto>>> Get()
        {
            var dtos = await _stateMachineService.Get();
            return ApiResponse.OK(dtos);
        }

        [HttpGet]
        public async Task<ApiResponse<StateMachineOutputDto>> Get([FromQuery]Guid id)
        {
            var dto = await _stateMachineService.Get(id);
            if (dto == null)
                return ApiResponse.NotFound("state machine not found", dto);
            return ApiResponse.OK(dto);
        }

        [HttpPut]
        public async Task<ApiResponse<PutResult<Guid>>> Put([FromBody]StateMachineInputDto dto)
        {
            var key = await _stateMachineService.Put(dto);
            var result = new PutResult<Guid>(key);
            return ApiResponse.OK(result);
        }

        [HttpPatch]
        public async Task<ApiResponse<PatchResult>> Patch([FromQuery] Guid id, [FromBody]StateMachineInputDto dto)
        {
            var patched = await _stateMachineService.Patch(id, dto);
            var result = new PatchResult(patched);
            if (dto == null)
                return ApiResponse.NotFound("state machine not found", result);
            return ApiResponse.OK(result);
        }

        [HttpDelete]
        public async Task<ApiResponse<DeleteResult>> Delete([FromQuery] Guid id)
        {
            var deleted = await _stateMachineService.Delete(id);
            var result = new DeleteResult(deleted);
            if (!deleted)
                return ApiResponse.NotFound("state machine not found", result);
            return ApiResponse.OK(result);
        }
    }
}
