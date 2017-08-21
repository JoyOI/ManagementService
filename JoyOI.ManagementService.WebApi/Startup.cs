using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.Configuration;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using Newtonsoft.Json;
using JoyOI.ManagementService.WebApi.WebApiModels;
using System.Text;

namespace JoyOI.ManagementService.WebApi
{
    public class Startup
    {
        private static IConfigurationRoot _configuration { get; set; }
        private static JoyOIManagementConfiguration _joyOIConfiguration { get; set; }
        internal static KestrelConfiguration _kestrelConfiguration { get; set; }

        internal class KestrelConfiguration
        {
            public int HttpsListenPort { get; set; }
            public string ServerCertificatePath { get; set; }
            public string ServerCertificatePassword { get; set; }
        }

        public Startup(IHostingEnvironment env)
        {
            // 读取配置
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            _configuration = builder.Build();

            // 生产模式时监听https
            if (env.IsProduction())
            {
                _kestrelConfiguration = new KestrelConfiguration();
                _configuration.GetSection("Kestrel").Bind(_kestrelConfiguration);
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwaggerGen(c =>
                c.SwaggerDoc("v1", new Info() { Title = "JoyOI Management Service", Version = "V1" }));

            JoyOIManagementContext.ConnectionString = _configuration.GetConnectionString("DefaultConnection");
            JoyOIManagementContext.MigrationAssembly = "JoyOI.ManagementService.WebApi";
            services.AddDbContext<JoyOIManagementContext>();

            var configuration = new JoyOIManagementConfiguration();
            _configuration.GetSection("JoyOIManagement").Bind(configuration);
            services.AddJoyOIManagement(configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(_configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // 添加swagger和错误页面
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "JoyOI Management Service V1"));
            }
            else
            {
                app.UseStatusCodePages();
            }

            // 全局处理mvc的错误, 发生错误时返回统一格式的json
            CatchExceptionAndReplyJson(app, env.IsDevelopment());

            // 使用WebApi
            app.UseMvc();

            // 启动管理服务
            app.ApplicationServices.StartJoyOIManagement();
        }

        private void CatchExceptionAndReplyJson(IApplicationBuilder app, bool isDevelopment)
        {
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    // 输出错误的详细信息到stderr
                    Console.Error.WriteLine($"{DateTime.Now}: {ex}");
                    Console.Error.Flush();
                    // 返回json
                    var json = JsonConvert.SerializeObject(
                        ApiResponse.InternalServerError(context.Response, ex, isDevelopment));
                    var jsonBytes = Encoding.UTF8.GetBytes(json);
                    context.Response.ContentType = "application/json; charset=utf-8";
                    context.Response.ContentLength = jsonBytes.Length;
                    await context.Response.Body.WriteAsync(jsonBytes, 0, jsonBytes.Length);
                }
            });
        }
    }
}
