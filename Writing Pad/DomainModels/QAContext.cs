using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Writing_Pad.DomainModels
{
    public partial class QAContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string databaseFilePath = "QA.sqlite";

            databaseFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseFilePath);
            options.UseSqlite($"Data source={databaseFilePath}");

            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HandWrittenNotes>(entity =>
            {
                entity.HasKey(e => e.Idx);
            });

            modelBuilder.Entity<RecognizedNotes>(entity =>
            {
                entity.HasKey(e => e.Idx);
            });
        }

        public virtual DbSet<HandWrittenNotes> HandWrittenNotes { get; set; }
        public virtual DbSet<RecognizedNotes> RecognizedNotes { get; set; }
    }
}
