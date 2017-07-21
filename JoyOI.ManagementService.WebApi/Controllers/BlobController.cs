using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Services;
using JoyOI.ManagementService.WebApi.WebApiModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

        [HttpGet("{id}")]
        public async Task<ApiResponse<BlobOutputDto>> Get(Guid id)
        {
            var dto = await _blobService.Get(id);
            if (dto == null)
                return ApiResponse.NotFound(Response, "blob not found", dto);
            return ApiResponse.OK(dto);
        }

        [HttpPut]
        public async Task<ApiResponse<PutResult<Guid>>> Put()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var json = await reader.ReadToEndAsync();
                var dto = JsonConvert.DeserializeObject<BlobInputDto>(json);
                var key = await _blobService.Put(dto);
                var result = new PutResult<Guid>(key);
                return ApiResponse.OK(result);
            }
        }
    }
}
