using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.WebApiModels
{
    public class PatchResult
    {
        public bool Patched { get; set; }

        public PatchResult()
        {

        }

        public PatchResult(bool patched)
        {
            Patched = patched;
        }
    }
}
