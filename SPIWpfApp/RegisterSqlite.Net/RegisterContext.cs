using Microsoft.EntityFrameworkCore;
using RegisterCore.Net.Models;
using System;

namespace RegisterSqlite.Net
{
    public class RegisterContext : DbContext
    {
        public const string DB_NAME = "register.sqlite";

        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Chip> Chips { get; set; }
        public DbSet<RegisterTemplate> RegisterTemplates { get; set; }
        public DbSet<BitFieldTemplate> BitFieldTemplates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            optionBuilder
                //.LogTo(Console.WriteLine)
                .EnableSensitiveDataLogging()
                .UseSqlite($"Data Source={DB_NAME}");
        }
    }
}
