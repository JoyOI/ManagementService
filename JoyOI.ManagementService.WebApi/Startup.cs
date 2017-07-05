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

namespace JoyOI.ManagementService.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwaggerGen(c =>
                c.SwaggerDoc("v1", new Info() { Title = "JoyOI Management Service", Version = "V1" }));

            JoyOIManagementContext.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
            JoyOIManagementContext.MigrationAssembly = "JoyOI.ManagementService.WebApi";
            services.AddDbContext<JoyOIManagementContext>();

            var configuration = new JoyOIManagementConfiguration();
            Configuration.GetSection("JoyOIManagement").Bind(configuration);
            services.AddJoyOIManagement(configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
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
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    context.Response.StatusCode = 200;
                    using (var writer = new StreamWriter(context.Response.Body))
                    {
                        var json = JsonConvert.SerializeObject(ApiResponse.InternalServerError(ex));
                        writer.Write(json);
                    }
                }
            });

            app.UseMvc();
        }
    }
}
