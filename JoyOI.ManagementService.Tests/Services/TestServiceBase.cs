using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Tests.Services
{
    public abstract class TestServiceBase : IDisposable
    {
        protected DummyStorage _storage;

        public TestServiceBase()
        {
            JoyOIManagementServiceCollectionExtensions.InitializeStaticFunctions();
            _storage = new DummyStorage();
        }

        public virtual void Dispose()
        {
        }
    }
}
