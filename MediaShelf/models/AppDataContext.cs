using System;
using Microsoft.EntityFrameworkCore;

namespace MediaShelf.Models
{
    public class AppDataContext : DbContext
    {
        public DbSet<User>? Usuarios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=MediaShelf.db");
        }
    }
}
