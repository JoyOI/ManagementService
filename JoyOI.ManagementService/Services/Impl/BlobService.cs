using JoyOI.ManagementService.Model.Dtos;
using JoyOI.ManagementService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services.Impl
{
    /// <summary>
    /// 管理文件的服务
    /// 文件需要分库储存, 不使用通用的基类
    /// </summary>
    internal class BlobService : IBlobService
    {
        public Task<bool> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IList<BlobOutputDto>> Get()
        {
            throw new NotImplementedException();
        }

        public Task<IList<BlobOutputDto>> Get(Expression<Func<BlobEntity, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public Task<BlobOutputDto> Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Patch(Guid id, BlobInputDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> Put(BlobInputDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
