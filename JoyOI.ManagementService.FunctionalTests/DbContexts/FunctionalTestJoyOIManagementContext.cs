using JoyOI.ManagementService.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.FunctionalTests.DbContexts
{
    public class FunctionalTestJoyOIManagementContext : JoyOIManagementContext
    {
        private string _databaseName;

        public FunctionalTestJoyOIManagementContext(string databaseName)
        {
            _databaseName = databaseName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseInMemoryDatabase(_databaseName);
        }
    }
}
