using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReversiApp.Models;

namespace ReversiApp.DAL
{
    public class ReversiContext : IdentityDbContext<Speler>
    {
        public ReversiContext(DbContextOptions<ReversiContext> options)
            : base(options)
        {
        }

        public DbSet<Speler> Speler { get; set; }
        public DbSet<Spel> Spel { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<SpelerSpel>().HasKey(ss => new { ss.SpelerId, ss.SpelID });
            builder.Entity<SpelerSpel>().HasOne(ss => ss.Speler).WithMany(s => s.SpelerSpel).HasForeignKey(ss => ss.SpelerId);
            builder.Entity<SpelerSpel>().HasOne(ss => ss.Spel).WithMany(s => s.SpelerSpel).HasForeignKey(ss => ss.SpelID);
        }

        public DbSet<ReversiApp.Models.SpelerSpel> SpelerSpel { get; set; }

    }
}
