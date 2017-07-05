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
    public class ActorController : Controller
    {
        private IActorService _actorService;

        public ActorController(IActorService actorService)
        {
            _actorService = actorService;
        }

        /// <summary>
        /// api/v1/Actor
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JoyOIApiResponse<IList<ActorOutputDto>>> Get()
        {
            var dtos = await _actorService.Get();
            return JoyOIApiResponse.OK(dtos);
        }
    }
}
