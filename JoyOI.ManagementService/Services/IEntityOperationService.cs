using JoyOI.ManagementService.Model.Dtos.Interfaces;
using JoyOI.ManagementService.Model.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 提供实体操作的服务接口
    /// </summary>
    public interface IEntityOperationService<TEntity, TPrimaryKey, TInputDto, TOutputDto>
        where TEntity : class, IEntity<TPrimaryKey>, new()
        where TInputDto : IInputDto
        where TOutputDto : IOutputDto
    {
        /// <summary>
        /// 获取符合条件的实体
        /// </summary>
        Task<IList<TOutputDto>> GetAll(Expression<Func<TEntity, bool>> expression);

        /// <summary>
        /// 获取单个符合条件的实体
        /// </summary>
        Task<TOutputDto> Get(Expression<Func<TEntity, bool>> expression);

        /// <summary>
        /// 添加单个实体
        /// </summary>
        Task<TPrimaryKey> Put(TInputDto dto);

        /// <summary>
        /// 覆盖单个实体, 返回覆盖数量(0或1)
        /// </summary>
        Task<long> Patch(Expression<Func<TEntity, bool>> expression, TInputDto dto);

        /// <summary>
        /// 删除符合条件的实体, 返回删除数量
        /// </summary>
        Task<long> Delete(Expression<Func<TEntity, bool>> expression);
    }

    /// <summary>
    /// 提供根据指定键操作实体的服务接口
    /// </summary>
    public interface IEntityOperationByKeyService<TEntity, TKey, TInputDto, TOutputDto>
        where TInputDto : IInputDto
        where TOutputDto : IOutputDto
    {
        /// <summary>
        /// 获取指定键的单个的实体
        /// </summary>
        Task<TOutputDto> Get(TKey key);

        /// <summary>
        /// 覆盖指定键的单个实体, 返回覆盖数量(0或1)
        /// </summary>
        Task<long> Patch(TKey key, TInputDto dto);

        /// <summary>
        /// 删除指定键的实体, 返回删除数量
        /// </summary>
        Task<long> Delete(TKey key);
    }
}
