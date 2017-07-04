﻿using System;
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

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "JoyOI Management Service V1"));
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePages();
            }
            app.UseMvc();
        }
    }
}
