using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Model.Entities.Interfaces
{
    /// <summary>
    /// 实体类的接口
    /// </summary>
    public interface IEntity
    {
    }

    /// <summary>
    /// 实体类的接口
    /// </summary>
    /// <typeparam name="TPrimaryKey">主键类型</typeparam>
    public interface IEntity<TPrimaryKey>
    {
        /// <summary>
        /// 实体Id
        /// </summary>
        TPrimaryKey Id { get; set; }
    }
}
