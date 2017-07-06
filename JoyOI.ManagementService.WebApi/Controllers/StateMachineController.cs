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
            var dtos = await _stateMachineService.GetAll(null);
            return ApiResponse.OK(dtos);
        }

        [HttpGet]
        public async Task<ApiResponse<StateMachineOutputDto>> Get([FromQuery]string name)
        {
            var dto = await _stateMachineService.Get(name);
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
        public async Task<ApiResponse<PatchResult>> Patch([FromQuery] string name, [FromBody]StateMachineInputDto dto)
        {
            var patched = await _stateMachineService.Patch(name, dto);
            var result = new PatchResult(patched);
            if (patched <= 0)
                return ApiResponse.NotFound("state machine not found", result);
            return ApiResponse.OK(result);
        }

        [HttpDelete]
        public async Task<ApiResponse<DeleteResult>> Delete([FromQuery] string name)
        {
            var deleted = await _stateMachineService.Delete(name);
            var result = new DeleteResult(deleted);
            if (deleted <= 0)
                return ApiResponse.NotFound("state machine not found", result);
            return ApiResponse.OK(result);
        }
    }
}
