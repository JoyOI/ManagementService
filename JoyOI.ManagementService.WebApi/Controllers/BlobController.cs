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
    public class BlobController : Controller
    {
        private IBlobService _blobService;

        public BlobController(IBlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpGet("All")]
        public async Task<ApiResponse<IList<BlobOutputDto>>> Get()
        {
            var dtos = await _blobService.GetAll(null);
            return ApiResponse.OK(dtos);
        }

        [HttpGet]
        public async Task<ApiResponse<BlobOutputDto>> Get([FromQuery]Guid id)
        {
            var dto = await _blobService.Get(id);
            if (dto == null)
                return ApiResponse.NotFound("blob not found", dto);
            return ApiResponse.OK(dto);
        }

        [HttpPut]
        public async Task<ApiResponse<PutResult<Guid>>> Put([FromBody]BlobInputDto dto)
        {
            var key = await _blobService.Put(dto);
            var result = new PutResult<Guid>(key);
            return ApiResponse.OK(result);
        }

        [HttpPatch]
        public async Task<ApiResponse<PatchResult>> Patch([FromQuery] Guid id, [FromBody]BlobInputDto dto)
        {
            var patched = await _blobService.Patch(id, dto);
            var result = new PatchResult(patched);
            if (patched <= 0)
                return ApiResponse.NotFound("blob not found", result);
            return ApiResponse.OK(result);
        }

        [HttpDelete]
        public async Task<ApiResponse<DeleteResult>> Delete([FromQuery] Guid id)
        {
            var deleted = await _blobService.Delete(id);
            var result = new DeleteResult(deleted);
            if (deleted <= 0)
                return ApiResponse.NotFound("blob not found", result);
            return ApiResponse.OK(result);
        }
    }
}
