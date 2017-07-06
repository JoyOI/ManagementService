using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.WebApiModels
{
    public class PatchResult
    {
        public long Patched { get; set; }

        public PatchResult()
        {

        }

        public PatchResult(long patched)
        {
            Patched = patched;
        }
    }
}
