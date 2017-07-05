using JoyOI.ManagementService.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.DbContexts
{
    /// <summary>
    /// 迁移用的数据库上下文
    /// EF Core 1.0时会执行Startup并且检测里面的上下文, 但EF Core 2.0则无法检测
    /// 因为EF Core 2.0正式版可能还会做出大更改, 目前为求简单, 直接写死连接字符串
    /// !!! 请勿注册这个类到ServiceCollection中 !!!
    /// !!! 迁移时请手动修改连接字符串, 重新编译, 然后执行dotnet ef database update !!!
    /// https://github.com/aspnet/EntityFramework/pull/8326
    /// https://github.com/aspnet/EntityFramework/issues/8164
    /// https://github.com/aspnet/EntityFramework/issues/7050
    /// https://github.com/aspnet/EntityFramework/issues/8888
    /// https://docs.microsoft.com/en-us/ef/core/get-started/aspnetcore/new-db
    /// </summary>
    public class MigrationJoyOIManagementContext : JoyOIManagementContext
    {
        public const string FixedConnectionString = "Server=127.0.0.1;Port=3306;Database=joyoi;User Id=root;Password=123456;";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(FixedConnectionString);
        }
    }
}
