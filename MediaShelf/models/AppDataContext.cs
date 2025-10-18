using System;
using Microsoft.EntityFrameworkCore;

namespace MediaShelf.Models
{
    public class AppDataContext : DbContext
    {
        public DbSet<User>? Usuarios { get; set; }
        public DbSet<Media>? Medias { get; set; }
        public DbSet<Review>? Reviews { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=MediaShelf.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar relacionamentos
            modelBuilder.Entity<Media>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Media)
                .WithMany(m => m.Reviews)
                .HasForeignKey(r => r.MediaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único: um usuário só pode avaliar uma mídia uma vez
            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.UserId, r.MediaId })
                .IsUnique();
        }
    }
}