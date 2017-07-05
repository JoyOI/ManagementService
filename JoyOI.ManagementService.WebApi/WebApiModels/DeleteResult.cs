using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.WebApiModels
{
    public class DeleteResult
    {
        public bool Deleted { get; set; }

        public DeleteResult()
        {

        }

        public DeleteResult(bool deleted)
        {
            Deleted = deleted;
        }
    }
}
