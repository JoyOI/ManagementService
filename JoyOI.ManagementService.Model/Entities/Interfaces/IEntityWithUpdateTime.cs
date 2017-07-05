using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Entities.Interfaces
{
    /// <summary>
    /// 带更新时间的实体类接口
    /// </summary>
    public interface IEntityWithUpdateTime : IEntity
    {
        DateTime UpdateTime { get; set; }
    }
}
