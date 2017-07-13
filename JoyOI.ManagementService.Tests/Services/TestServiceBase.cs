using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.Tests.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Tests.Services
{
    public class TestServiceBase : IDisposable
    {
        protected JoyOIManagementContext _context;

        public TestServiceBase()
        {
            JoyOIManagementServiceCollectionExtensions.InitializeStaticFunctions();
            _context = new TestJoyOIManagementContext();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
