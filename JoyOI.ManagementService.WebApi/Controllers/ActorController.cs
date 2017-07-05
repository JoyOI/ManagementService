using JoyOI.ManagementService.WebApi.WebApiModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class ActorController : Controller
    {
        /// <summary>
        /// api/v1/Actor
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JoyOIApiResponse> Get()
        {
            throw new NotImplementedException();
        }
    }
}
