using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 管理文件的服务接口
    /// </summary>
    public interface IBlobService :
        IEntityOperationService<BlobEntity, Guid, BlobInputDto, BlobOutputDto>,
        IEntityOperationByKeyService<BlobEntity, Guid, BlobInputDto, BlobOutputDto>
    {
    }
}
