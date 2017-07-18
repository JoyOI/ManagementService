using JoyOI.ManagementService.Model.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 管理状态机实例的服务接口
    /// </summary>
    public interface IStateMachineInstanceService
    {
        /// <summary>
        /// 搜索状态机实例
        /// </summary>
        Task<IList<StateMachineInstanceOutputDto>> Search(
            string name, string stage, string status, string begin_time, string finish_time);

        /// <summary>
        /// 获取状态机实例
        /// </summary>
        Task<StateMachineInstanceOutputDto> Get(Guid id);

        /// <summary>
        /// 创建状态机实例
        /// </summary>
        Task<StateMachineInstancePutResultDto> Put(StateMachineInstancePutDto dto);

        /// <summary>
        /// 更新状态机实例
        /// </summary>
        Task<StateMachineInstancePatchResultDto> Patch(Guid id, StateMachineInstancePatchDto dto);

        /// <summary>
        /// 删除状态机实例
        /// </summary>
        Task<long> Delete(Guid id);
    }
}
