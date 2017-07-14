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
            // fix #3: GET /blob/all needs ignore body in response
            // I can make it faster, but this api shouldn't use in normal case at all, so I just took the simple way
            foreach (var dto in dtos)
            {
                dto.Body = null;
            }
            return ApiResponse.OK(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ApiResponse<BlobOutputDto>> Get(Guid id)
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
    }
}
