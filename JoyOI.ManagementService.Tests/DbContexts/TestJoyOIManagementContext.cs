using JoyOI.ManagementService.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace JoyOI.ManagementService.Tests.DbContexts
{
    public class TestJoyOIManagementContext : JoyOIManagementContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            // Warning as error exception for warning
            // Transactions are not supported by the in-memory store
            optionsBuilder.ConfigureWarnings(x => x.Default(WarningBehavior.Ignore));
        }
    }
}
