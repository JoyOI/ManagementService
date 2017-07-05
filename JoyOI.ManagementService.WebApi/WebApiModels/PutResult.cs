using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.WebApiModels
{
    public class PutResult<TPrimaryKey>
    {
        public TPrimaryKey Id { get; set; }

        public PutResult()
        {

        }

        public PutResult(TPrimaryKey id)
        {
            Id = id;
        }
    }
}
