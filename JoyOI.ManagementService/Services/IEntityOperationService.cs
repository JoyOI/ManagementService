using JoyOI.ManagementService.Model.Dtos.Interfaces;
using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 提供实体操作的服务接口
    /// </summary>
    public interface IEntityOperationService<TEntity, TPrimaryKey, TInputDto, TOutputDto>
        where TEntity : class, IEntity<TPrimaryKey>
        where TInputDto : IInputDto
        where TOutputDto : IOutputDto
    {
        /// <summary>
        /// 获取所有实体
        /// </summary>
        Task<IList<TOutputDto>> Get();

        /// <summary>
        /// 获取符合条件的实体
        /// </summary>
        Task<IList<TOutputDto>> Get(Expression<Func<TEntity, bool>> expression);

        /// <summary>
        /// 获取单个实体
        /// </summary>
        Task<TOutputDto> Get(TPrimaryKey id);

        /// <summary>
        /// 添加单个实体
        /// </summary>
        Task<TPrimaryKey> Put(TInputDto dto);

        /// <summary>
        /// 覆盖单个实体, 返回是否覆盖成功
        /// </summary>
        Task<bool> Patch(TPrimaryKey id, TInputDto dto);

        /// <summary>
        /// 删除单个实体, 返回是否删除成功
        /// </summary>
        Task<bool> Delete(TPrimaryKey id);
    }
}
