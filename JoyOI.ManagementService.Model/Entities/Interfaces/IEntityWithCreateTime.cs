using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Entities.Interfaces
{
    /// <summary>
    /// 带创建时间的实体类接口
    /// </summary>
    public interface IEntityWithCreateTime : IEntity
    {
        DateTime CreateTime { get; set; }
    }
}
