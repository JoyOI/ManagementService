using JoyOI.ManagementService.Model.Dtos.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Dtos
{
    public class BlobOutputDto : IOutputDto
    {
        public Guid Id { get; set; }
        // base64
        public string Body { get; set; }
        public long TimeStamp { get; set; }
        public string Remark { get; set; }
    }
}
