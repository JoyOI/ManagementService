using JoyOI.ManagementService.Configuration;
using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.FunctionalTests.DbContexts;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JoyOI.ManagementService.FunctionalTests.Services
{
    public abstract class TestServiceBase : IDisposable
    {
        protected JoyOIManagementConfiguration _configuration { get; set; }
        protected string _databaseName { get; set; }
        protected Func<JoyOIManagementContext> _contextFactory { get; set; }

        public TestServiceBase()
        {
            while (!File.Exists(Path.Combine(Environment.CurrentDirectory, "appsettings.json")))
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(Environment.CurrentDirectory);
            }
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            _configuration = new JoyOIManagementConfiguration();
            configuration.GetSection("JoyOIManagement").Bind(_configuration);
            _configuration.AfterLoaded();
            _databaseName = Guid.NewGuid().ToString();
            _contextFactory = () => new FunctionalTestJoyOIManagementContext(_databaseName);
        }

        public void Dispose()
        {
        }
    }
}
