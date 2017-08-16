using JoyOI.ManagementService.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.WebApi.DbContexts
{
    /// <summary>
    /// 迁移用的数据库上下文
    /// </summary>
    public class MigrationJoyOIManagementContext : JoyOIManagementContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var basePath = AppContext.BaseDirectory;
            string jsonPath;
            do {
                if (string.IsNullOrEmpty(basePath))
                    throw new FileNotFoundException("no parent folder contains appsettings.json");
                jsonPath = Path.Combine(basePath, "appsettings.json");
                basePath = Path.GetDirectoryName(basePath);
            } while (!File.Exists(jsonPath));
            var builder = new ConfigurationBuilder()
                .AddJsonFile(jsonPath, optional: false)
                .AddEnvironmentVariables();
            var configuration = builder.Build();
            var connectionString = configuration.GetSection("ConnectionStrings").GetValue<string>("DefaultConnection");
            optionsBuilder.UseMySql(connectionString);
        }
    }
}
