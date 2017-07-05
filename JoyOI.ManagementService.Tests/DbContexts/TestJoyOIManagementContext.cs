using JoyOI.ManagementService.DbContexts;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace JoyOI.ManagementService.Tests.DbContexts
{
    public class TestJoyOIManagementContext : JoyOIManagementContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("TestJoyOIManagementDatabase");
        }
    }
}
