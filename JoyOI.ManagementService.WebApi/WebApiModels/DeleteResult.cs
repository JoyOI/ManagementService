using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.WebApiModels
{
    public class DeleteResult
    {
        public long Deleted { get; set; }

        public DeleteResult()
        {

        }

        public DeleteResult(long deleted)
        {
            Deleted = deleted;
        }
    }
}
