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
    public class StateMachineInstanceController : Controller
    {
        private IStateMachineInstanceService _stateMachineInstanceService;

        public StateMachineInstanceController(IStateMachineInstanceService stateMachineInstanceService)
        {
            _stateMachineInstanceService = stateMachineInstanceService;
        }

        [HttpGet("Search")]
        public async Task<ApiResponse<IList<StateMachineInstanceOutputDto>>> Search(
            [FromQuery]string name, [FromQuery]string currentActor)
        {
            var dtos = await _stateMachineInstanceService.Search(name, currentActor);
            return ApiResponse.OK(dtos);
        }

        [HttpGet]
        public async Task<ApiResponse<StateMachineInstanceOutputDto>> Get([FromQuery]Guid id)
        {
            var dto = await _stateMachineInstanceService.Get(id);
            if (dto == null)
                return ApiResponse.NotFound("state machine instance not found", dto);
            return ApiResponse.OK(dto);
        }

        [HttpPut]
        public async Task<ApiResponse<PutResult<Guid>>> Put([FromBody]StateMachineInstancePutDto dto)
        {
            var putResult = await _stateMachineInstanceService.Put(dto);
            var result = new PutResult<Guid>(putResult.Instance?.Id ?? Guid.Empty);
            return ApiResponse.Custom(putResult.Code, putResult.Message, result);
        }

        [HttpPatch]
        public async Task<ApiResponse<PatchResult>> Patch(
            [FromQuery] Guid id, [FromBody]StateMachineInstancePatchDto dto)
        {
            var patchResult = await _stateMachineInstanceService.Patch(id, dto);
            var result = new PatchResult(patchResult.Code == 200 ? 1 : 0);
            return ApiResponse.Custom(patchResult.Code, patchResult.Message, result);
        }

        [HttpDelete]
        public async Task<ApiResponse<DeleteResult>> Delete([FromQuery] Guid id)
        {
            var deleted = await _stateMachineInstanceService.Delete(id);
            var result = new DeleteResult(deleted);
            if (deleted <= 0)
                return ApiResponse.NotFound("state machine instance not found", result);
            return ApiResponse.OK(result);
        }
    }
}
