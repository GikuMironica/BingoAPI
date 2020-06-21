using BingoAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Data
{
    public class ErrorDataContext : IdentityDbContext
    {
        public ErrorDataContext(DbContextOptions<ErrorDataContext> options) : base(options)
        {

        }

        public DbSet<ErrorLog> ErrorLogs { get; set; }
    }

}
